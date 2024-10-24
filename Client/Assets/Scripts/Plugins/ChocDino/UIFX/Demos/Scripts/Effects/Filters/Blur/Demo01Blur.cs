//--------------------------------------------------------------------------//
// Copyright 2023-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using UnityEngine;
using UnityEngine.UI;
using ChocDino.UIFX;

namespace ChocDino.UIFX.Demos
{
	public class Demo01Blur : MonoBehaviour
	{
		[Header("Elements")]
		[SerializeField] BlurFilter _blur = null;

		[Header("UI")]
		[SerializeField] Button _buttonBoth = null;
		[SerializeField] Button _buttonHoriz = null;
		[SerializeField] Button _buttonVert = null;
		[SerializeField] Slider _sliderStrength = null;

		void Start()
		{
			Time.timeScale = 1f;

			// Create UI
			_buttonBoth.onClick.AddListener(OnButtonBothClick);
			_buttonHoriz.onClick.AddListener(OnButtonHorizontalClick);
			_buttonVert.onClick.AddListener(OnButtonVerticalClick);
			_sliderStrength.onValueChanged.AddListener(OnSliderMotionSpeed);

			// Set initial mode
			OnButtonBothClick();
			OnSliderMotionSpeed(_sliderStrength.value);
		}

		void OnButtonBothClick()
		{
			_blur.BlurAxes2D = BlurAxes2D.Default;

			_buttonBoth.image.color = Color.yellow;
			_buttonHoriz.image.color = Color.white;
			_buttonVert.image.color = Color.white;
		}

		void OnButtonHorizontalClick()
		{
			_blur.BlurAxes2D = BlurAxes2D.Horizontal;

			_buttonBoth.image.color = Color.white;
			_buttonHoriz.image.color = Color.yellow;
			_buttonVert.image.color = Color.white;
		}

		void OnButtonVerticalClick()
		{
			_blur.BlurAxes2D = BlurAxes2D.Vertical;

			_buttonBoth.image.color = Color.white;
			_buttonHoriz.image.color = Color.white;
			_buttonVert.image.color = Color.yellow;
		}

		void OnSliderMotionSpeed(float value)
		{
			_blur.Strength = value;
		}
	}
}
