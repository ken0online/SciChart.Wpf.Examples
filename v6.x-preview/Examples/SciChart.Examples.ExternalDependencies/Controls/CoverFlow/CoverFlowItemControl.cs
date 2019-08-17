// *************************************************************************************
// SCICHART® Copyright SciChart Ltd. 2011-2019. All rights reserved.
//  
// Web: http://www.scichart.com
//   Support: support@scichart.com
//   Sales:   sales@scichart.com
// 
// CoverFlowItemControl.cs is part of SCICHART®, High Performance Scientific Charts
// For full terms and conditions of the license, see http://www.scichart.com/scichart-eula/
// 
// This source code is protected by international copyright law. Unauthorized
// reproduction, reverse-engineering, or distribution of all or any portion of
// this source code is strictly prohibited.
// 
// This source code contains confidential and proprietary trade secrets of
// SciChart Ltd., and should at no time be copied, transferred, sold,
// distributed or made available without express written permission.
// *************************************************************************************
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SciChart.Examples.ExternalDependencies.Controls.CoverFlow
{
    [TemplateVisualState(GroupName = "MouseStates", Name = "MouseEnter")]
    [TemplateVisualState(GroupName = "MouseStates", Name = "MouseLeave")]
    [TemplateVisualState(GroupName = "SelectedStates", Name = "SelectedState")]
    [TemplateVisualState(GroupName = "SelectedStates", Name = "UnselectedState")]
    public class CoverFlowItemControl : ListBoxItem, IDisposable
    {
#if SILVERLIGHT
        private PlaneProjection _planeProjection;
#else
        private PlaneProjector _planeProjection;
#endif
        private bool _isItemSelected;

        private double _x;
        private double _yRotation;
        private double _scale;
        private bool _isAnimating;

        private Border _container;

        private Duration _duration;
        private IEasingFunction _easingFunction;
        private DoubleAnimation _xAnimation;

        private Storyboard _animation;
        private ScaleTransform _scaleTransform;
        private EasingDoubleKeyFrame _rotationKeyFrame;
        private EasingDoubleKeyFrame _scaleXKeyFrame;
        private EasingDoubleKeyFrame _scaleYKeyFrame;

        public event EventHandler ItemSelected;

        public bool IsItemSelected
        {
            get { return _isItemSelected; }
            set
            {
                _isItemSelected = value;
                IsSelected = value;

                var state = value ? "SelectedState" : "UnselectedState";
                UpdateVisualState(state, true);
            }
        }

        public double YRotation
        {
            get { return _yRotation; }
            set
            {
                _yRotation = value;
                if (_planeProjection != null)
                {
                    _planeProjection.RotationY = value;
                }
            }
        }

        public double Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                if (_scaleTransform != null)
                {
                    _scaleTransform.ScaleX = _scale;
                    _scaleTransform.ScaleY = _scale;
                }
            }
        }

        public double X
        {
            get { return _x; }
            set
            {
                _x = value;
                Canvas.SetLeft(this, value);
            }
        }

        public CoverFlowItemControl()
        {
            DefaultStyleKey = typeof (CoverFlowItemControl);

            MouseEnter += (sender, args) => UpdateVisualState("MouseEnter", true);
            MouseLeave += (sender, args) => UpdateVisualState("MouseLeave", true);
        }

        private void UpdateVisualState(string state, bool useTransition)
        {
            VisualStateManager.GoToState(this, state, useTransition);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _container = (Border)GetTemplateChild("container");

            _rotationKeyFrame = (EasingDoubleKeyFrame)GetTemplateChild("rotationKeyFrame");
            _scaleXKeyFrame = (EasingDoubleKeyFrame)GetTemplateChild("scaleXKeyFrame");
            _scaleYKeyFrame = (EasingDoubleKeyFrame)GetTemplateChild("scaleYKeyFrame");
            _scaleTransform = (ScaleTransform)GetTemplateChild("scaleTransform");

#if SILVERLIGHT
            _planeProjection = (PlaneProjection)GetTemplateChild("Rotator");
#else
            _planeProjection = (PlaneProjector)GetTemplateChild("Rotator");
#endif

            if (_planeProjection != null)
            {
                _planeProjection.RotationY = _yRotation;
                if (_container != null)
                {
                    SubscribeContentPresenterEvents(_container);
                }
            }

            _animation = (Storyboard)GetTemplateChild("Animation");
            if (_animation != null)
            {
                SubscribeAnimationEvents(_animation);

                _xAnimation = new DoubleAnimation();
                _animation.Children.Add(_xAnimation);

                Storyboard.SetTarget(_xAnimation, this);
                Storyboard.SetTargetProperty(_xAnimation, new PropertyPath("(Canvas.Left)"));
            }

            UpdateVisualState("MouseLeave", true);
        }

        public void Dispose()
        {
            UnsubscribeContentPresenterEvents(_container);
            UnsubscribeAnimationEvents(_animation);
        }

        private void SubscribeContentPresenterEvents(FrameworkElement contentPresenter)
        {
            UnsubscribeContentPresenterEvents(contentPresenter);

            if (contentPresenter != null)
            {
                contentPresenter.MouseLeftButtonUp += ContentPresenter_MouseLeftButtonUp;
            }
        }

        private void UnsubscribeContentPresenterEvents(FrameworkElement contentPresenter)
        {
            if (contentPresenter != null)
            {
                contentPresenter.MouseLeftButtonUp -= ContentPresenter_MouseLeftButtonUp;
            }
        }

        private void SubscribeAnimationEvents(Storyboard storyboard)
        {
            UnsubscribeAnimationEvents(storyboard);

            if (storyboard != null)
            {
                storyboard.Completed += Animation_Completed;
            }
        }

        private void UnsubscribeAnimationEvents(Storyboard storyboard)
        {
            if (storyboard != null)
            {
                storyboard.Completed -= Animation_Completed;
            }
        }

        void Animation_Completed(object sender, EventArgs e)
        {
            _isAnimating = false;
        }

        void ContentPresenter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ItemSelected != null)
            {
                ItemSelected(this, null);
            }
        }

        public void SetValues(double x, int zIndex, double r, double z, double s, Duration d, IEasingFunction ease, bool useAnimation)
        {
            if (useAnimation)
            {
                if (!_isAnimating && Canvas.GetLeft(this) != x)
                {
                    Canvas.SetLeft(this, _x);
                }

                _rotationKeyFrame.Value = r;
                _scaleYKeyFrame.Value = s;
                _scaleXKeyFrame.Value = s;
                _xAnimation.To = x;

                if (_duration != d)
                {
                    _duration = d;
                    _rotationKeyFrame.KeyTime = KeyTime.FromTimeSpan(d.TimeSpan);
                    _scaleYKeyFrame.KeyTime = KeyTime.FromTimeSpan(d.TimeSpan);
                    _scaleXKeyFrame.KeyTime = KeyTime.FromTimeSpan(d.TimeSpan);
                    _xAnimation.Duration = d;
                }
                if (_easingFunction != ease)
                {
                    _easingFunction = ease;
                    _rotationKeyFrame.EasingFunction = ease;
                    _scaleYKeyFrame.EasingFunction = ease;
                    _scaleXKeyFrame.EasingFunction = ease;
                    _xAnimation.EasingFunction = ease;
                }

                _isAnimating = true;
                _animation.Begin();
                Canvas.SetZIndex(this, zIndex);
            }

            _x = x;
        }
    }
}