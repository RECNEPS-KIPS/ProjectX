// author:KIPKIPS
// date:2024.10.26 18:42
// describe:
using System;
using Framework.Core.Container;

namespace Framework.Core.SpaceSegment
{
    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TreeType
{
	/// <summary>
	/// 线性四叉树
	/// </summary>
	LinearQuadTree,
	/// <summary>
	/// 线性八叉树
	/// </summary>
	LinearOctree,
	/// <summary>
	/// 四叉树
	/// </summary>
	QuadTree,
	/// <summary>
	/// 八叉树
	/// </summary>
	Octree,
}

/// <summary>
/// 场景物件加载控制器
/// </summary>
public class ScenableLoadController : MonoBehaviour
{

    private WaitForEndOfFrame m_WaitForFrame;

    /// <summary>
    /// 当前场景资源四叉树/八叉树
    /// </summary>
    private ITree<Scenable> m_Tree;

    /// <summary>
    /// 刷新时间
    /// </summary>
    private float m_RefreshTime;
    /// <summary>
    /// 销毁时间
    /// </summary>
    private float m_DestroyRefreshTime;
    
    private Vector3 m_OldRefreshPosition;
    private Vector3 m_OldDestroyRefreshPosition;

    /// <summary>
    /// 异步任务队列
    /// </summary>
    private Queue<Scenable> m_ProcessTaskQueue;

	/// <summary>
	/// 已加载的物体列表（频繁移除与添加使用双向链表）
	/// </summary>
	private LinkedList<Scenable> m_LoadedObjectLinkedList;

    /// <summary>
    /// 待销毁物体列表
    /// </summary>
    private PriorityQueue<Scenable> m_PreDestroyObjectQueue;

    private TriggerHandle<Scenable> m_TriggerHandle;

    private bool m_IsTaskRunning;

    private bool m_IsInitialized;

    private int m_MaxCreateCount;
    private int m_MinCreateCount;
    private float m_MaxRefreshTime;
    private float m_MaxDestroyTime;
    private bool m_Async;

    private IDetector m_CurrentDetector;
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="center">场景区域中心</param>
    /// <param name="size">场景区域大小</param>
    /// <param name="async">是否异步</param>
    /// <param name="maxCreateCount">最大创建数量</param>
    /// <param name="minCreateCount">最小创建数量</param>
    /// <param name="maxRefreshTime">更新区域时间间隔</param>
    /// <param name="maxDestroyTime">检查销毁时间间隔</param>
    /// <param name="treeType"></param>
    /// <param name="quadTreeDepth">四叉树深度</param>
    public void Init(Vector3 center, Vector3 size, bool async, int maxCreateCount, int minCreateCount, float maxRefreshTime, float maxDestroyTime, TreeType treeType , int quadTreeDepth = 5)
    {
        if (m_IsInitialized)
        {
            return;
        }

        m_Tree = treeType switch
        {
            TreeType.LinearOctree => new LinearOctree<Scenable>(center, size, quadTreeDepth),
            TreeType.LinearQuadTree => new LinearQuadTree<Scenable>(center, size, quadTreeDepth),
            TreeType.Octree => new SceneTree<Scenable>(center, size, quadTreeDepth, true),
            TreeType.QuadTree => new SceneTree<Scenable>(center, size, quadTreeDepth, false),
            _ => new LinearQuadTree<Scenable>(center, size, quadTreeDepth)
        };

        m_LoadedObjectLinkedList = new LinkedList<Scenable>();
        m_PreDestroyObjectQueue = new PriorityQueue<Scenable>(new SceneObjectWeightComparer());
        m_TriggerHandle = TriggerHandle; 

        m_MaxCreateCount = Mathf.Max(0, maxCreateCount);
        m_MinCreateCount = Mathf.Clamp(minCreateCount, 0, m_MaxCreateCount);
        m_MaxRefreshTime = maxRefreshTime;
        m_MaxDestroyTime = maxDestroyTime;
        m_Async = async;

        m_IsInitialized = true;

        m_RefreshTime = maxRefreshTime;
    }
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="center">场景区域中心</param>
    /// <param name="size">场景区域大小</param>
    /// <param name="async">是否异步</param>
    /// <param name="treeType"></param>
    public void Init(Vector3 center, Vector3 size, bool async, TreeType treeType)
    {
        Init(center, size, async, 25, 15, 1, 5, treeType);
    }
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="center">场景区域中心</param>
    /// <param name="size">场景区域大小</param>
    /// <param name="async">是否异步</param>
    /// <param name="maxCreateCount">更新区域时间间隔</param>
    /// <param name="minCreateCount">检查销毁时间间隔</param>
    /// <param name="treeType"></param>
    public void Init(Vector3 center, Vector3 size, bool async, int maxCreateCount, int minCreateCount, TreeType treeType)
    {
        Init(center, size, async, maxCreateCount, minCreateCount, 1, 5, treeType);
    }

    void OnDestroy()
    {
        if (m_Tree != null)
            m_Tree.Clear();
        m_Tree = null;
        if (m_ProcessTaskQueue != null)
            m_ProcessTaskQueue.Clear();
	    if (m_LoadedObjectLinkedList != null)
		    m_LoadedObjectLinkedList.Clear();
		m_ProcessTaskQueue = null;
	    m_LoadedObjectLinkedList = null;
		m_TriggerHandle = null;
    }

    /// <summary>
    /// 添加场景物体
    /// </summary>
    /// <param name="obj"></param>
    public void AddSceneBlockObject(IScenable obj)
    {
        if (!m_IsInitialized || m_Tree == null || obj == null)
        {
            return;
        }

        //使用SceneObject包装
        Scenable sceneBlockObj = new Scenable(obj);
        m_Tree.Add(sceneBlockObj);
        //如果当前触发器存在，直接物体是否可触发，如果可触发，则创建物体
        if (m_CurrentDetector != null && m_CurrentDetector.IsDetected(sceneBlockObj.Bounds))
        {
            DoCreateInternal(sceneBlockObj);
        }
    }

    /// <summary>
    /// 刷新触发器
    /// </summary>
    /// <param name="detector">触发器</param>
    public void RefreshDetector(IDetector detector)
    {
        if (!m_IsInitialized)
        {
            return;
        }
        //只有坐标发生改变才调用
        if (m_OldRefreshPosition != detector.Position)
        {
            m_RefreshTime += Time.deltaTime;
            //达到刷新时间才刷新，避免区域更新频繁
            if (m_RefreshTime > m_MaxRefreshTime)
            {
                m_OldRefreshPosition = detector.Position;
                m_RefreshTime = 0;
                m_CurrentDetector = detector;
                //进行触发检测
                m_Tree.Trigger(detector, m_TriggerHandle);
                //标记超出区域的物体
                MarkOutOfBoundsObjs();
                //m_IsInitLoadComplete = true;
            }
        }
        if (m_OldDestroyRefreshPosition != detector.Position)
        {
            if(m_PreDestroyObjectQueue != null && m_PreDestroyObjectQueue.Count >= m_MaxCreateCount && m_PreDestroyObjectQueue.Count > m_MinCreateCount)
            //if (m_PreDestroyObjectList != null && m_PreDestroyObjectList.Count >= m_MaxCreateCount)
            {
                m_DestroyRefreshTime += Time.deltaTime;
                if (m_DestroyRefreshTime > m_MaxDestroyTime)
                {
                    m_OldDestroyRefreshPosition = detector.Position;
                    m_DestroyRefreshTime = 0;
                    //删除超出区域的物体
                    DestroyOutOfBoundsObjs();
                }
            }
        }
    }

    /// <summary>
    /// 四叉树触发处理函数
    /// </summary>
    /// <param name="data">与当前包围盒发生触发的场景物体</param>
    void TriggerHandle(Scenable data)
    {
        if (data == null)
        {
            return;
        }
        if (data.Flag == Scenable.CreateFlag.Old) //如果发生触发的物体已经被创建则标记为新物体，以确保不会被删掉
        {
            data.Weight ++;
            data.Flag = Scenable.CreateFlag.New;
        }
        else if (data.Flag == Scenable.CreateFlag.OutOfBounds)//如果发生触发的物体已经被标记为超出区域，则从待删除列表移除该物体，并标记为新物体
        {
            data.Flag = Scenable.CreateFlag.New;
            //if (m_PreDestroyObjectList.Remove(data))
            {
	            m_LoadedObjectLinkedList.AddFirst(data);
            }
        }
        else if (data.Flag == Scenable.CreateFlag.None) //如果发生触发的物体未创建则创建该物体并加入已加载的物体列表
        {
            DoCreateInternal(data);
        }
    }

    //执行创建物体
    private void DoCreateInternal(Scenable data)
	{
		//加入已加载列表
		m_LoadedObjectLinkedList.AddFirst(data);
        //创建物体
        CreateObject(data, m_Async);
    }

    /// <summary>
    /// 标记离开视野的物体
    /// </summary>
    private void MarkOutOfBoundsObjs()
    {
        if (m_LoadedObjectLinkedList == null)
        {
            return;
        }

	    var node = m_LoadedObjectLinkedList.First;
	    while (node != null)
	    {
		    var obj = node.Value;
		    if (obj.Flag == Scenable.CreateFlag.Old)//已加载物体标记仍然为Old，说明该物体没有进入触发区域，即该物体在区域外
		    {
			    obj.Flag = Scenable.CreateFlag.OutOfBounds;
			    if (m_MinCreateCount == 0)//如果最小创建数为0直接删除
			    {
				    DestroyObject(obj, m_Async);
			    }
			    else
			    {
				    m_PreDestroyObjectQueue.Push(obj);//加入待删除队列
			    }

			    var next = node.Next;
			    m_LoadedObjectLinkedList.Remove(node);
				node = next;
			}
		    else
		    {
			    obj.Flag = Scenable.CreateFlag.Old;
			    node = node.Next;
		    }
	    }
    }

    /// <summary>
    /// 删除超出区域外的物体
    /// </summary>
    private void DestroyOutOfBoundsObjs()
    {
        while(m_PreDestroyObjectQueue.Count>m_MinCreateCount)
        {
            var obj = m_PreDestroyObjectQueue.Pop();
            if (obj == null)
                continue;
            if (obj.Flag == Scenable.CreateFlag.OutOfBounds)
            {
                DestroyObject(obj, m_Async);
            }
        }
    }

    /// <summary>
    /// 创建物体
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="async"></param>
    private void CreateObject(Scenable obj, bool async)
    {
        if (obj?.TargetObj == null)
        {
            return;
        }
        if (obj.Flag != Scenable.CreateFlag.None) return;
        if (!async)
        {
            CreateObjectSync(obj);
        } else
        {
            ProcessObjectAsync(obj, true);
        }
        obj.Flag = Scenable.CreateFlag.New;//被创建的物体标记为New
    }

    /// <summary>
    /// 删除物体
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="async"></param>
    private void DestroyObject(Scenable obj, bool async)
    {
        if (obj == null || obj.Flag == Scenable.CreateFlag.None || obj.TargetObj == null)
        {
            return;
        }
        if (!async)
        {
            DestroyObjectSync(obj);
        } else
        {
            ProcessObjectAsync(obj, false);
        }
        obj.Flag = Scenable.CreateFlag.None;//被删除的物体标记为None
    }

    /// <summary>
    /// 同步方式创建物体
    /// </summary>
    /// <param name="obj"></param>
    private void CreateObjectSync(Scenable obj)
    {
        if (obj.ProcessFlag == Scenable.CreatingProcessFlag.IsPrepareDestroy)//如果标记为IsPrepareDestroy表示物体已经创建并正在等待删除，则直接设为None并返回
        {
            obj.ProcessFlag = Scenable.CreatingProcessFlag.None;
            return;
        }
        obj.OnShow(transform);//执行OnShow
    }

    /// <summary>
    /// 同步方式销毁物体
    /// </summary>
    /// <param name="obj"></param>
    private void DestroyObjectSync(Scenable obj)
    {
        if (obj.ProcessFlag == Scenable.CreatingProcessFlag.IsPrepareCreate)//如果物体标记为IsPrepareCreate表示物体未创建并正在等待创建，则直接设为None并放回
        {
            obj.ProcessFlag = Scenable.CreatingProcessFlag.None;
            return;
        }
        obj.OnHide();//执行OnHide
    }

    /// <summary>
    /// 异步处理
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="create"></param>
    private void ProcessObjectAsync(Scenable obj, bool create)
    {
        if (create)
        {
            if (obj.ProcessFlag == Scenable.CreatingProcessFlag.IsPrepareDestroy)//表示物体已经创建并等待销毁，则设置为None并跳过
            {
                obj.ProcessFlag = Scenable.CreatingProcessFlag.None;
                return;
            }
            if (obj.ProcessFlag == Scenable.CreatingProcessFlag.IsPrepareCreate) //已经开始等待创建，则跳过
            {
                return;
            }
            obj.ProcessFlag = Scenable.CreatingProcessFlag.IsPrepareCreate;//设置为等待开始创建
        }
        else
        {
            if (obj.ProcessFlag == Scenable.CreatingProcessFlag.IsPrepareCreate)//表示物体未创建并等待创建，则设置为None并跳过
            {
                obj.ProcessFlag = Scenable.CreatingProcessFlag.None;
                return;
            }
            if (obj.ProcessFlag == Scenable.CreatingProcessFlag.IsPrepareDestroy) //已经开始等待销毁，则跳过
            {
                return;
            }
            obj.ProcessFlag = Scenable.CreatingProcessFlag.IsPrepareDestroy;//设置为等待开始销毁
        }
        m_ProcessTaskQueue ??= new Queue<Scenable>();
        m_ProcessTaskQueue.Enqueue(obj);//加入
        if (!m_IsTaskRunning)
        {
            StartCoroutine(AsyncTaskProcess());//开始协程执行异步任务
        }
    }

    /// <summary>
    /// 异步任务
    /// </summary>
    /// <returns></returns>
    private IEnumerator AsyncTaskProcess()
    {
        if (m_ProcessTaskQueue == null)
        {
            yield return 0;
        }
        m_IsTaskRunning = true;
        while (m_ProcessTaskQueue.Count > 0)
        {
            var obj = m_ProcessTaskQueue.Dequeue();
            if (obj != null)
            {
                if (obj.ProcessFlag == Scenable.CreatingProcessFlag.IsPrepareCreate)//等待创建
                {
                    obj.ProcessFlag = Scenable.CreatingProcessFlag.None;
                    if (obj.OnShow(transform))
                    {
                        m_WaitForFrame ??= new WaitForEndOfFrame();
                        yield return m_WaitForFrame;
                    }
                }
                else if (obj.ProcessFlag == Scenable.CreatingProcessFlag.IsPrepareDestroy)//等待销毁
                {
                    obj.ProcessFlag = Scenable.CreatingProcessFlag.None;
                    obj.OnHide();
                    m_WaitForFrame ??= new WaitForEndOfFrame();
                    yield return m_WaitForFrame;
                }
            }
        }
        m_IsTaskRunning = false;
    }

    private class SceneObjectWeightComparer : IComparer<Scenable>
    {

        public int Compare(Scenable x, Scenable y)
        {
            if (y.Weight < x.Weight)
            {
                return 1;
            }
            if (Math.Abs(y.Weight - x.Weight) < 0.01f)
            {
                return 0;
            }
            return -1;
        }
    }

#if UNITY_EDITOR
    public int debugDrawMinDepth;
    public int debugDrawMaxDepth = 5;
    public bool debugDrawObj = true;
    void OnDrawGizmosSelected()
    {
        Color minColor = new Color32(0, 66, 255, 255);
        Color maxColor = new Color32(133, 165, 255, 255);
        Color objColor = new Color32(0, 210, 255, 255);
        Color hitColor = new Color32(255, 216, 0, 255);
        m_Tree?.DrawTree(minColor, maxColor, objColor, hitColor, debugDrawMinDepth, debugDrawMaxDepth, debugDrawObj);
    }
#endif
}

}