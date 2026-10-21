using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace StadiaStudio {
    public sealed class AccentState : DependencyObject {
        public static readonly DependencyProperty ValueProperty=DependencyProperty.Register("Value",typeof(Color),typeof(AccentState),new PropertyMetadata(Colors.Transparent));
        public Color Value {get{return (Color)GetValue(ValueProperty);}set{SetValue(ValueProperty,value);}}
    }
    public static class Ui {
        static readonly AccentState accentState=new AccentState{Value=Color("#B5E879")};
        static SolidColorBrush AccentBrush(){var brush=new SolidColorBrush();System.Windows.Data.BindingOperations.SetBinding(brush,SolidColorBrush.ColorProperty,new System.Windows.Data.Binding("Value"){Source=accentState});return brush;}
        public static Color Color(string value){return (Color)ColorConverter.ConvertFromString(value);}
        public static SolidColorBrush Brush(string value){SolidColorBrush b=new SolidColorBrush(Color(value));b.Freeze();return b;}
        public static readonly Brush Background=Brush("#202020"),Panel=Brush("#292929"),Inset=Brush("#191919"),Line=Brush("#3C3C3C"),Text=Brush("#F5F5F5"),Muted=Brush("#B5B5B5"),Lime=AccentBrush(),Coral=Brush("#FFB4A4");
        public static readonly string[] AccentColors={"#B5E879","#8CC8FF","#76DDCF","#C6B6FF","#FFB3CA","#F4CA82"};
        public static void SetAccent(string value,bool persist) {
            if(Array.IndexOf(AccentColors,value)<0)return;
            if(persist)using(var key=Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\StadiaStudio")){key.SetValue("AccentColor",value);}
            accentState.Value=Color(value);
        }
        static Ui(){using(var key=Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\StadiaStudio")){if(key!=null)SetAccent((string)key.GetValue("AccentColor","#B5E879"),false);}}
        public static TextBlock T(string text,double size,Brush color=null,bool bold=false) {
            TextBlock block=new TextBlock{Text=I18n.T(text),FontSize=size,Foreground=color??Text,FontWeight=bold?FontWeights.SemiBold:FontWeights.Normal,TextWrapping=TextWrapping.Wrap};I18n.Remember(block,text);return block;
        }
        public static Border Card(UIElement child,int padding=24) {
            Border card=new Border{Background=Panel,BorderBrush=Line,BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(8),Padding=new Thickness(padding),Child=child};
            SolidColorBrush edge=new SolidColorBrush(Color("#3C3C3C"));card.BorderBrush=edge;
            card.MouseEnter+=(s,e)=>{if(SystemParameters.ClientAreaAnimation)edge.BeginAnimation(SolidColorBrush.ColorProperty,new ColorAnimation(Color("#555555"),TimeSpan.FromMilliseconds(150)));else edge.Color=Color("#555555");};
            card.MouseLeave+=(s,e)=>{if(SystemParameters.ClientAreaAnimation)edge.BeginAnimation(SolidColorBrush.ColorProperty,new ColorAnimation(Color("#3C3C3C"),TimeSpan.FromMilliseconds(180)));else edge.Color=Color("#3C3C3C");};
            return card;
        }
        public static void Enter(FrameworkElement view) {
            if(!SystemParameters.ClientAreaAnimation)return;
            TranslateTransform shift=new TranslateTransform();view.RenderTransform=shift;
            view.BeginAnimation(UIElement.OpacityProperty,new DoubleAnimation(.35,1,TimeSpan.FromMilliseconds(160)){EasingFunction=new CubicEase{EasingMode=EasingMode.EaseOut}});
            shift.BeginAnimation(TranslateTransform.YProperty,new DoubleAnimation(6,0,TimeSpan.FromMilliseconds(180)){EasingFunction=new QuadraticEase{EasingMode=EasingMode.EaseOut}});
        }
        public static Button Button(string label,Action click,bool accent=false) {
            Button b=new Button{Content=I18n.T(label),MinHeight=40,Padding=new Thickness(17,8,17,8),HorizontalContentAlignment=HorizontalAlignment.Center,Background=accent?Lime:Inset,Foreground=accent?Background:Text,
                BorderBrush=accent?Lime:Line,FontSize=13,FontWeight=FontWeights.SemiBold,Cursor=System.Windows.Input.Cursors.Hand};
            ScaleTransform scale=new ScaleTransform(1,1);b.RenderTransform=scale;b.RenderTransformOrigin=new Point(.5,.5);
            b.PreviewMouseLeftButtonDown+=(s,e)=>{if(!SystemParameters.ClientAreaAnimation)return;scale.BeginAnimation(ScaleTransform.ScaleXProperty,new DoubleAnimation(.97,TimeSpan.FromMilliseconds(70)));scale.BeginAnimation(ScaleTransform.ScaleYProperty,new DoubleAnimation(.97,TimeSpan.FromMilliseconds(70)));};
            b.PreviewMouseLeftButtonUp+=(s,e)=>Release(scale);b.MouseLeave+=(s,e)=>Release(scale);
            I18n.Remember(b,label);b.Click+=(s,e)=>click();return b;
        }
        static void Release(ScaleTransform scale){if(!SystemParameters.ClientAreaAnimation){scale.BeginAnimation(ScaleTransform.ScaleXProperty,null);scale.BeginAnimation(ScaleTransform.ScaleYProperty,null);scale.ScaleX=scale.ScaleY=1;return;}scale.BeginAnimation(ScaleTransform.ScaleXProperty,new DoubleAnimation(1,TimeSpan.FromMilliseconds(120)));scale.BeginAnimation(ScaleTransform.ScaleYProperty,new DoubleAnimation(1,TimeSpan.FromMilliseconds(120)));}
        public static void Styles(ResourceDictionary resources,bool setup=false) {
            string xaml=@"<ResourceDictionary xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
<Style TargetType='Button'><Setter Property='BorderThickness' Value='1'/><Setter Property='Template'><Setter.Value><ControlTemplate TargetType='Button'><Border x:Name='B' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}' BorderThickness='{TemplateBinding BorderThickness}' CornerRadius='10' Padding='{TemplateBinding Padding}'><ContentPresenter HorizontalAlignment='{TemplateBinding HorizontalContentAlignment}' VerticalAlignment='Center'/></Border><ControlTemplate.Triggers><Trigger Property='IsMouseOver' Value='True'><Setter TargetName='B' Property='Opacity' Value='.8'/></Trigger><Trigger Property='IsEnabled' Value='False'><Setter TargetName='B' Property='Opacity' Value='.4'/></Trigger><Trigger Property='IsKeyboardFocused' Value='True'><Setter TargetName='B' Property='BorderBrush' Value='#CAF57A'/></Trigger></ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter></Style>
<Style TargetType='ComboBox'><Setter Property='Foreground' Value='#E9EEEB'/><Setter Property='Background' Value='#131B1E'/><Setter Property='BorderBrush' Value='#3C3C3C'/><Setter Property='Padding' Value='12,8'/><Setter Property='MinHeight' Value='38'/><Setter Property='Template'><Setter.Value><ControlTemplate TargetType='ComboBox'><Grid><ToggleButton x:Name='Toggle' IsChecked='{Binding IsDropDownOpen,RelativeSource={RelativeSource TemplatedParent},Mode=TwoWay}' Focusable='False' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}' Foreground='{TemplateBinding Foreground}'><ToggleButton.Template><ControlTemplate TargetType='ToggleButton'><Border Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}' BorderThickness='1' CornerRadius='8'><Path Data='M1 3 L6 8 L11 3' Width='10' Height='6' Stretch='Fill' Stroke='#B5B5B5' StrokeThickness='1.3' HorizontalAlignment='Right' VerticalAlignment='Center' Margin='0,0,12,0'/></Border></ControlTemplate></ToggleButton.Template></ToggleButton><ContentPresenter Content='{TemplateBinding SelectionBoxItem}' ContentTemplate='{TemplateBinding SelectionBoxItemTemplate}' Margin='12,8,30,8' IsHitTestVisible='False' VerticalAlignment='Center'/><Popup x:Name='PART_Popup' IsOpen='{TemplateBinding IsDropDownOpen}' Placement='Bottom' AllowsTransparency='True' Focusable='False'><Border Background='#202A2E' BorderBrush='#46554F' BorderThickness='1' CornerRadius='8' Padding='4' MinWidth='{Binding ActualWidth,RelativeSource={RelativeSource TemplatedParent}}'><ScrollViewer MaxHeight='320'><ItemsPresenter/></ScrollViewer></Border></Popup></Grid></ControlTemplate></Setter.Value></Setter></Style>
<Style TargetType='ComboBoxItem'><Setter Property='Foreground' Value='#E9EEEB'/><Setter Property='Padding' Value='10,7'/><Setter Property='Template'><Setter.Value><ControlTemplate TargetType='ComboBoxItem'><Border x:Name='B' Padding='{TemplateBinding Padding}' CornerRadius='5'><ContentPresenter/></Border><ControlTemplate.Triggers><Trigger Property='IsHighlighted' Value='True'><Setter TargetName='B' Property='Background' Value='#3D4D42'/></Trigger><Trigger Property='IsSelected' Value='True'><Setter TargetName='B' Property='Background' Value='#3D4D42'/></Trigger></ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter></Style>
<Style TargetType='TextBox'><Setter Property='Background' Value='#131B1E'/><Setter Property='Foreground' Value='#E9EEEB'/><Setter Property='BorderBrush' Value='#3C3C3C'/><Setter Property='CaretBrush' Value='#CAF57A'/><Setter Property='Padding' Value='12,9'/><Setter Property='FontSize' Value='14'/><Setter Property='Template'><Setter.Value><ControlTemplate TargetType='TextBox'><Border Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}' BorderThickness='1' CornerRadius='8' Padding='{TemplateBinding Padding}'><ScrollViewer x:Name='PART_ContentHost'/></Border></ControlTemplate></Setter.Value></Setter></Style>

<Style TargetType='Slider'><Setter Property='IsMoveToPointEnabled' Value='True'/><Setter Property='Template'><Setter.Value><ControlTemplate TargetType='Slider'><Grid Height='22' Background='Transparent'><Track x:Name='PART_Track' Minimum='{TemplateBinding Minimum}' Maximum='{TemplateBinding Maximum}' Value='{TemplateBinding Value}'><Track.DecreaseRepeatButton><RepeatButton Command='Slider.DecreaseLarge' Focusable='False'><RepeatButton.Template><ControlTemplate><Border Height='4' CornerRadius='2' Background='#CAF57A'/></ControlTemplate></RepeatButton.Template></RepeatButton></Track.DecreaseRepeatButton><Track.IncreaseRepeatButton><RepeatButton Command='Slider.IncreaseLarge' Focusable='False'><RepeatButton.Template><ControlTemplate><Border Height='4' CornerRadius='2' Background='#3B494B'/></ControlTemplate></RepeatButton.Template></RepeatButton></Track.IncreaseRepeatButton><Track.Thumb><Thumb Width='16' Height='16' Cursor='Hand'><Thumb.Template><ControlTemplate><Ellipse Fill='#CAF57A' Stroke='#101517' StrokeThickness='2'/></ControlTemplate></Thumb.Template></Thumb></Track.Thumb></Track></Grid></ControlTemplate></Setter.Value></Setter></Style>
<Style TargetType='CheckBox'><Setter Property='Foreground' Value='#E9EEEB'/><Setter Property='Margin' Value='0,10,0,0'/><Setter Property='FontSize' Value='13'/><Setter Property='Template'><Setter.Value><ControlTemplate TargetType='CheckBox'><StackPanel Orientation='Horizontal'><Border x:Name='Box' Width='18' Height='18' Background='#131B1E' BorderBrush='#52625A' BorderThickness='1' CornerRadius='4'><TextBlock x:Name='Tick' Text='✓' Visibility='Collapsed' Foreground='#101517' HorizontalAlignment='Center' VerticalAlignment='Center' FontWeight='Bold'/></Border><ContentPresenter Margin='9,0,0,0' VerticalAlignment='Center'/></StackPanel><ControlTemplate.Triggers><Trigger Property='IsChecked' Value='True'><Setter TargetName='Box' Property='Background' Value='#CAF57A'/><Setter TargetName='Box' Property='BorderBrush' Value='#CAF57A'/><Setter TargetName='Tick' Property='Visibility' Value='Visible'/></Trigger><Trigger Property='IsKeyboardFocused' Value='True'><Setter TargetName='Box' Property='BorderBrush' Value='#CAF57A'/></Trigger></ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter></Style>
<Style TargetType='ScrollBar'><Setter Property='Width' Value='12'/><Setter Property='Template'><Setter.Value><ControlTemplate TargetType='ScrollBar'><Grid Background='Transparent' Margin='2,4'><Track x:Name='PART_Track' IsDirectionReversed='True' Orientation='Vertical' Minimum='{TemplateBinding Minimum}' Maximum='{TemplateBinding Maximum}' Value='{TemplateBinding Value}' ViewportSize='{TemplateBinding ViewportSize}'><Track.DecreaseRepeatButton><RepeatButton Command='ScrollBar.PageUpCommand' Focusable='False'><RepeatButton.Template><ControlTemplate><Border Background='Transparent'/></ControlTemplate></RepeatButton.Template></RepeatButton></Track.DecreaseRepeatButton><Track.IncreaseRepeatButton><RepeatButton Command='ScrollBar.PageDownCommand' Focusable='False'><RepeatButton.Template><ControlTemplate><Border Background='Transparent'/></ControlTemplate></RepeatButton.Template></RepeatButton></Track.IncreaseRepeatButton><Track.Thumb><Thumb MinHeight='28' Background='#52645D'><Thumb.Template><ControlTemplate TargetType='Thumb'><Border x:Name='Grip' Background='{TemplateBinding Background}' CornerRadius='3' Margin='1,0'/><ControlTemplate.Triggers><Trigger Property='IsMouseOver' Value='True'><Setter TargetName='Grip' Property='Background' Value='#91AD9B'/></Trigger><Trigger Property='IsDragging' Value='True'><Setter TargetName='Grip' Property='Background' Value='#CAF57A'/></Trigger></ControlTemplate.Triggers></ControlTemplate></Thumb.Template></Thumb></Track.Thumb></Track></Grid></ControlTemplate></Setter.Value></Setter></Style>
<Style TargetType='ScrollViewer'><Setter Property='Padding' Value='0,0,10,0'/><Setter Property='VerticalScrollBarVisibility' Value='Auto'/><Setter Property='HorizontalScrollBarVisibility' Value='Disabled'/></Style>
</ResourceDictionary>";
            xaml=xaml.Replace("#101517","#202020").Replace("#131B1E","#303030").Replace("#E9EEEB","#F5F5F5").Replace("#98AAA9","#B5B5B5").Replace("#202A2E","#292929").Replace("#46554F","#555555").Replace("#3D4D42","#3D3D3D").Replace("#3B494B","#555555").Replace("#52625A","#888888").Replace("#52645D","#777777").Replace("#91AD9B","#AAAAAA").Replace("CornerRadius='10'","CornerRadius='4'").Replace("CornerRadius='8'","CornerRadius='4'");
            if(!setup){resources["AppAccent"]=Lime;xaml=xaml.Replace("#CAF57A","{DynamicResource AppAccent}");}
            if(setup)xaml=xaml.Replace("#CAF57A","#60CDFF").Replace("#101517","#202020").Replace("#131B1E","#292929").Replace("#E9EEEB","#F5F5F5").Replace("CornerRadius='10'","CornerRadius='4'");
            resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Parse(xaml));
        }
        public static void VerifyAccent(){string original=accentState.Value.ToString();try{foreach(string value in AccentColors){SetAccent(value,false);if(((SolidColorBrush)Lime).Color!=Color(value)||Lime.IsFrozen)throw new Exception("Live accent update failed: "+value);}}finally{accentState.Value=Color(original);}}
        public static FrameworkElement Gap(int pixels){return new Border{Height=pixels};}
        public static FrameworkElement Logo(double size){
            using(var stream=System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("logo.xaml")) {
                return new Viewbox{Width=size,Height=size,Child=(FrameworkElement)XamlReader.Load(stream),HorizontalAlignment=HorizontalAlignment.Left};
            }
        }
    }
}
