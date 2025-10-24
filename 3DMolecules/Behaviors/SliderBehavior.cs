using System;
using System.Windows;
using System.Windows.Controls;

namespace ThreeDMolecules.Behaviors
{
 /// <summary>
 /// Slider behavior that creates a magnetic snap to a specified center value within a threshold,
 /// while preserving linear, continuous movement elsewhere.
 /// </summary>
 public static class SliderBehavior
 {
 public static readonly DependencyProperty CenterValueProperty = DependencyProperty.RegisterAttached(
 "CenterValue",
 typeof(double),
 typeof(SliderBehavior),
 new PropertyMetadata(0.0));

 public static void SetCenterValue(DependencyObject element, double value) => element.SetValue(CenterValueProperty, value);
 public static double GetCenterValue(DependencyObject element) => (double)element.GetValue(CenterValueProperty);

 public static readonly DependencyProperty CenterSnapThresholdProperty = DependencyProperty.RegisterAttached(
 "CenterSnapThreshold",
 typeof(double),
 typeof(SliderBehavior),
 new PropertyMetadata(0.0, OnCenterSnapThresholdChanged));

 public static void SetCenterSnapThreshold(DependencyObject element, double value) => element.SetValue(CenterSnapThresholdProperty, value);
 public static double GetCenterSnapThreshold(DependencyObject element) => (double)element.GetValue(CenterSnapThresholdProperty);

 private static readonly DependencyProperty IsSnappingProperty = DependencyProperty.RegisterAttached(
 "IsSnapping",
 typeof(bool),
 typeof(SliderBehavior),
 new PropertyMetadata(false));

 private static void SetIsSnapping(DependencyObject element, bool value) => element.SetValue(IsSnappingProperty, value);
 private static bool GetIsSnapping(DependencyObject element) => (bool)element.GetValue(IsSnappingProperty);

 private static void OnCenterSnapThresholdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
 {
 if (d is not Slider slider) return;

 // Detach previous handler if any
 slider.ValueChanged -= SliderOnValueChanged;

 var threshold = (double)e.NewValue;
 if (threshold >0)
 {
 slider.ValueChanged += SliderOnValueChanged;
 }
 }

 private static void SliderOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
 {
 if (sender is not Slider slider) return;
 if (GetIsSnapping(slider)) return; // prevent re-entry

 double center = GetCenterValue(slider);
 double threshold = GetCenterSnapThreshold(slider);
 if (threshold <=0) return;

 double value = e.NewValue;
 if (Math.Abs(value - center) <= threshold)
 {
 try
 {
 SetIsSnapping(slider, true);
 slider.Value = center; // snap to center
 }
 finally
 {
 SetIsSnapping(slider, false);
 }
 }
 }
 }
}
