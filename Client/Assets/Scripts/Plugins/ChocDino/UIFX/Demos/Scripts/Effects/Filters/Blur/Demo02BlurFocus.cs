//--------------------------------------------------------------------------//
// Copyright 2023-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using UnityEngine;
using UnityEngine.UI;
using ChocDino.UIFX;

namespace ChocDino.UIFX.Demos
{
	/// <summary>
	/// Shift "focus" between two UI elements by changing the blur strength
	/// </summary>
	public class Demo02BlurFocus : MonoBehaviour
	{
		[SerializeField] float _speed = 1f;
		[SerializeField] BlurFilter[] _blurItems = null;

		private float _depthMin;
		private float _depthMax;

		void Start()
		{
			Time.timeScale = 1f;

			_depthMin = float.MaxValue;
			_depthMax = float.MinValue;
			foreach (var blur in _blurItems)
			{
				_depthMin = Mathf.Min(_depthMin, blur.transform.localPosition.z);
				_depthMax = Mathf.Max(_depthMax, blur.transform.localPosition.z);
			}
		}
		
		void Update()
		{
			float t = Mathf.Sin(Time.time * _speed);
			t = (t * 0.5f) + 0.5f;
			t = Mathf.Clamp01(t);
			t = DemoUtils.InOutExpo(t);

			float tt = Mathf.Lerp(_depthMin, _depthMax, t);
			float range = (_depthMax - _depthMin);

			foreach (var blur in _blurItems)
			{
				float d = Mathf.Abs(blur.transform.localPosition.z - tt);
				blur.Strength = Mathf.Clamp01(d / range);
			}
		}
	}
}
