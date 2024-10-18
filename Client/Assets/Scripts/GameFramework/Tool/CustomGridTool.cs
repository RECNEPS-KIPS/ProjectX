using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Color = UnityEngine.Color;

namespace GameLogic
{
    public enum eGetGridPosType
    {
        Row,
        Column,
        All,
    }
    
    public class CustomGrid
    {
        private Vector2 m_min;
        private Vector2 m_max;
        private Vector2 m_spaceOffset;
        private Vector2 m_cellSize;
        
        private Vector2Int m_gridSize;
        private Vector2 m_orginPos;
        
        public int GridColumn => m_gridSize.y;
        public int GridRow => m_gridSize.x;
        public Vector2 SpaceOffset => m_spaceOffset;
        public Vector2 CellSize => m_cellSize;

        public CustomGrid(Vector2 min,Vector2 max,Vector2 spaceOffset,Vector2 cellSize,Vector2 orginPos)
        {
            m_min = min;
            m_max = max;
            m_spaceOffset = spaceOffset;
            m_cellSize = cellSize;

            var width = m_max.x - m_min.x;
            var height = m_max.y - m_min.y;
            m_gridSize = new Vector2Int(Mathf.FloorToInt((width + spaceOffset.x) / (cellSize.x + spaceOffset.x)), Mathf.FloorToInt((height + spaceOffset.y) / (cellSize.y + spaceOffset.y)));
            m_orginPos = orginPos;
        }

        public static void DrawGizmos(Vector2 min,Vector2 max,Vector2 cellSize,Vector2 spaceOffset,Vector2 originPos)
        {
            Gizmos.color = Color.red;
            var rectMin = (Vector3)(originPos + min);
            var rectMax = (Vector3)(originPos + max);
            var width = max.x - min.x;
            var height = max.y - min.y;
            var gridSize = new Vector2Int(Mathf.FloorToInt((width + spaceOffset.x) / (cellSize.x + spaceOffset.x)), Mathf.FloorToInt((height + spaceOffset.y) / (cellSize.y + spaceOffset.y)));
            
            Gizmos.DrawLine(rectMin,new Vector3(rectMin.x,rectMax.y));
            Gizmos.DrawLine(rectMin,new Vector3(rectMax.x,rectMin.y));
            Gizmos.DrawLine(rectMax,new Vector3(rectMax.x,rectMin.y));
            Gizmos.DrawLine(rectMax,new Vector3(rectMin.x,rectMax.y));
            
            Gizmos.color = Color.black;
            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    var cellMin = GetCellWorldPos(i, j, min, cellSize, spaceOffset, originPos);
                    var cellMax = cellMin + cellSize;
                    Gizmos.DrawLine(cellMin,new Vector3(cellMax.x,cellMin.y));
                    Gizmos.DrawLine(cellMin,new Vector3(cellMin.x,cellMax.y));
                    Gizmos.DrawLine(new Vector3(cellMax.x,cellMin.y),cellMax);
                    Gizmos.DrawLine(new Vector3(cellMin.x,cellMax.y),cellMax);
                }
            }
        }

        public Vector2Int GetGridPos(int index)
        {
            return new Vector2Int(index / GridRow, index % GridColumn);
        }
        
        public List<Vector2Int> GetGridsCellByType(int index,eGetGridPosType getType)
        {
            return GetGridsCellByType(new Vector2Int(index / GridRow, index % GridColumn), getType);
        }

        public List<Vector2Int> GetGridsCellByType(Vector2Int pos,eGetGridPosType getType)
        {
            List<Vector2Int> resultPosList = new List<Vector2Int>();
            switch (getType)
            {
                case eGetGridPosType.Column:
                    for (int i = 0; i < m_gridSize.y; i++)
                    {
                        resultPosList.Add(new Vector2Int(pos.x,i));
                    }
                    break;
                case eGetGridPosType.Row:
                    for (int i = 0; i < m_gridSize.x; i++)
                    {
                        resultPosList.Add(new Vector2Int(i,pos.y));
                    }
                    break;
                case eGetGridPosType.All:
                    for (int i = 0; i < m_gridSize.y; i++)
                    {
                        for (int j = 0; j < m_gridSize.x; j++)
                        {
                            resultPosList.Add(new Vector2Int(j,i));
                        }
                    }
                    break;
            }

            return resultPosList;
        }
        
        /// <summary>
        /// BottomToTop LeftToRight
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector2 GetCellWorldPos(int index)
        {
            return GetCellWorldPos(index / GridRow, index % GridColumn);
        }
        
        public Vector2 GetCellWorldPos(int row,int column)
        {
            return GetCellWorldPos(row, column, m_min, m_cellSize, m_spaceOffset, m_orginPos);
        }
        
        public Vector2 GetCellLocalPos(int row,int column)
        {
            return GetCellLocalPos(row, column, m_min, m_cellSize, m_spaceOffset);
        }
        
        public static Vector2 GetCellWorldPos(int row,int column,Vector2 min,Vector2 cellSize,Vector2 spaceOffset,Vector2 originPos)
        {
            return GetCellLocalPos(row, column, min, cellSize, spaceOffset) + originPos;
        }
        
        public static Vector2 GetCellLocalPos(int row,int column,Vector2 min,Vector2 cellSize,Vector2 spaceOffset)
        {
            return new Vector2(cellSize.x * row , cellSize.y * column) + spaceOffset.x * new Vector2(row,0)+ spaceOffset.y * new Vector2(0,column) +  min;
        }
    }
    
    public class CustomGridTool : MonoBehaviour
    {
        private CustomGrid m_customGrid;
        public Vector2 min;
        public Vector2 max;
        public Vector2 spaceOffset;
        public Vector2 cellSize;

        public CustomGrid Grid => m_customGrid;
        public int GridRow => m_customGrid?.GridRow ?? 0;
        public int GridColumn => m_customGrid?.GridColumn ?? 0;


        public void CreateGrid()
        {
            m_customGrid = new CustomGrid(min, max, spaceOffset, cellSize,transform.position);
        }

        private void OnDrawGizmos()
        {
            CustomGrid.DrawGizmos(min,max,cellSize,spaceOffset,transform.position);
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(CustomGridTool))]
    public class CustomGridEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var customGridTool = (CustomGridTool)target;
            EditorGUILayout.LabelField("GridSize",new Vector2(customGridTool.GridRow,customGridTool.GridColumn).ToString());
        }
    }
    
#endif
}

