using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
namespace StadiaStudio {
 public sealed class ControllerVisual:UserControl {
  readonly Canvas drawing=new Canvas{Width=555,Height=395};
  readonly Dictionary<int,Border> zones=new Dictionary<int,Border>();
  readonly Dictionary<int,Path> sculpted=new Dictionary<int,Path>();
  readonly Dictionary<int,TextBlock> labels=new Dictionary<int,TextBlock>();
  readonly Dictionary<int,Path> directionMarks=new Dictionary<int,Path>();
  readonly TranslateTransform left=new TranslateTransform(),right=new TranslateTransform();
  readonly Brush idle=Ui.Brush("#F1F2EB"),outline=Ui.Brush("#A1A8A3"),accent=Ui.Lime;
  int selected=-1;bool[] pressed=new bool[19];readonly SolidColorBrush[] triggerInk={new SolidColorBrush(),new SolidColorBrush()};public event Action<int> Selected;
  public ControllerVisual() {
   SculptedZone(17,"M87,28 L87,14 Q125,-1 170,14 L170,28 Z",128,15,"L2");
   SculptedZone(18,"M386,28 L386,14 Q430,-1 469,14 L469,28 Z",427,15,"R2");
   ShapePath("M98,28 C160,25 392,25 455,28 C490,32 511,60 524,111 C540,163 553,245 552,318 C551,349 533,372 506,371 C474,370 450,347 425,303 C405,267 391,251 366,250 L188,250 C164,250 148,267 129,303 C104,347 80,370 48,371 C20,372 3,349 2,318 C1,245 15,164 31,111 C45,60 64,32 98,28 Z");
   SculptedZone(4,"M70,53 Q82,28 101,28 L170,28 L170,46 Q121,41 70,53 Z",128,37,"L1");
   SculptedZone(5,"M386,28 L454,28 Q474,28 486,53 Q436,41 386,46 Z",427,37,"R1");
   ShapePath("M114,77 Q114,68 127,68 Q140,68 140,77 L140,99 L160,99 Q173,99 173,113 Q173,126 160,126 L140,126 L140,148 Q140,159 127,159 Q114,159 114,148 L114,126 L92,126 Q81,126 81,113 Q81,99 92,99 L114,99 Z",true);
   SculptedZone(13,"M114,77 Q114,68 127,68 Q140,68 140,77 L140,100 L127,113 L114,100 Z",0,0,null);
   SculptedZone(14,"M127,113 L140,126 L140,148 Q140,159 127,159 Q114,159 114,148 L114,126 Z",0,0,null);
   SculptedZone(15,"M127,113 L114,126 L92,126 Q81,126 81,113 Q81,99 92,99 L114,99 Z",0,0,null);
   SculptedZone(16,"M127,113 L140,99 L160,99 Q173,99 173,113 Q173,126 160,126 L140,126 Z",0,0,null);
   DirectionMark(13,"M123,86 L127,82 L131,86");DirectionMark(14,"M123,141 L127,145 L131,141");
   DirectionMark(15,"M99,109 L95,113 L99,117");DirectionMark(16,"M155,109 L159,113 L155,117");
   Zone(0,416,132,36,36,true,"A");Zone(1,451,97,36,36,true,"B");Zone(2,381,97,36,36,true,"X");Zone(3,416,62,36,36,true,"Y");
   Zone(6,198,67,37,24,false,"•••");Zone(7,321,67,38,24,false,"≡");
   Zone(11,302,103,29,29,true,"▣");Zone(12,225,103,29,29,true,"");Zone(10,258,177,40,40,true,"");
   Detail("M267 191 Q278 184 290 190 L286 195 Q277 191 269 197 Z M270 200 Q278 196 285 199 L280 205 Q275 202 271 205 Z");
   Detail("M239 116 A4 4 0 1 1 231 116 A4 4 0 1 1 239 116 Z M245.5 120 A2.5 2.5 0 1 1 240.5 120 A2.5 2.5 0 1 1 245.5 120 Z M245.8 113 A1.8 1.8 0 1 1 242.2 113 A1.8 1.8 0 1 1 245.8 113 Z");
   Stick(8,196,197,left);Stick(9,359,197,right);
   Content=new Viewbox{Child=drawing,Stretch=Stretch.Uniform};
  }
  void Detail(string data){drawing.Children.Add(new Path{Data=Geometry.Parse(data),Fill=Ui.Brush("#5C6460"),IsHitTestVisible=false});}
  void ShapePath(string data,bool dpad=false){Geometry geometry=Geometry.Parse(data);geometry.Freeze();drawing.Children.Add(new Path{Data=geometry,Fill=dpad?Ui.Brush("#434849"):(Brush)new LinearGradientBrush(Ui.Color("#FCFCF7"),Ui.Color("#C9CECA"),new Point(0,0),new Point(.35,1)),Stroke=dpad?Ui.Brush("#202526"):outline,StrokeThickness=2,IsHitTestVisible=false});}
  void SculptedZone(int id,string data,double x,double y,string label){Geometry geometry=Geometry.Parse(data);geometry.Freeze();Path shape=new Path{Data=geometry,Fill=label==null?Brushes.Transparent:Idle(id),Stroke=label==null?Brushes.Transparent:outline,StrokeThickness=1.5,StrokeLineJoin=PenLineJoin.Round,ToolTip=Destination.Physical[id],Cursor=System.Windows.Input.Cursors.Hand};shape.MouseLeftButtonDown+=(s,e)=>{Select(id);if(Selected!=null)Selected(id);e.Handled=true;};System.Windows.Automation.AutomationProperties.SetName(shape,Destination.Physical[id]);shape.Focusable=true;shape.KeyDown+=(s,e)=>{if(e.Key==System.Windows.Input.Key.Enter||e.Key==System.Windows.Input.Key.Space){Select(id);if(Selected!=null)Selected(id);e.Handled=true;}};shape.GotKeyboardFocus+=(s,e)=>Select(id);drawing.Children.Add(shape);sculpted[id]=shape;if(label!=null){TextBlock text=new TextBlock{Text=label,FontSize=13,FontWeight=FontWeights.SemiBold,Foreground=id>=17?Ui.Text:Ui.Brush("#383B3D"),Width=40,TextAlignment=TextAlignment.Center,IsHitTestVisible=false};Canvas.SetLeft(text,x-20);Canvas.SetTop(text,y-9);drawing.Children.Add(text);labels[id]=text;}}
  void DirectionMark(int id,string data){Path mark=new Path{Data=Geometry.Parse(data),Stroke=outline,StrokeThickness=2,StrokeStartLineCap=PenLineCap.Round,StrokeEndLineCap=PenLineCap.Round,StrokeLineJoin=PenLineJoin.Round,IsHitTestVisible=false};drawing.Children.Add(mark);directionMarks[id]=mark;}
  void Stick(int id,double x,double y,TranslateTransform movement){Ellipse ring=new Ellipse{Width=76,Height=76,Fill=Ui.Brush("#E47C3B"),Stroke=Ui.Brush("#C2642B"),StrokeThickness=2,IsHitTestVisible=false};Canvas.SetLeft(ring,x-38);Canvas.SetTop(ring,y-38);drawing.Children.Add(ring);Zone(id,x-27,y-27,54,54,true,"");zones[id].RenderTransform=movement;Ellipse inner=new Ellipse{Width=40,Height=40,Stroke=outline,StrokeThickness=1,IsHitTestVisible=false,RenderTransform=movement};Canvas.SetLeft(inner,x-20);Canvas.SetTop(inner,y-20);drawing.Children.Add(inner);}
  Brush Idle(int id){return id==8||id==9?Ui.Brush("#292D2E"):id>=17?Ui.Brush("#414747"):id==4||id==5?Ui.Brush("#B9C1BD"):idle;}
  void Zone(int id,double x,double y,double width,double height,bool circle,string label){Border z=new Border{Width=width,Height=height,CornerRadius=new CornerRadius(circle?width/2:6),Background=Idle(id),BorderBrush=outline,BorderThickness=new Thickness(1.5),ToolTip=Destination.Physical[id],Cursor=System.Windows.Input.Cursors.Hand,Child=new TextBlock{Text=label,Foreground=Ui.Brush("#383B3D"),FontSize=height<25?10:14,FontWeight=FontWeights.SemiBold,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center,IsHitTestVisible=false}};z.MouseLeftButtonDown+=(s,e)=>{Select(id);if(Selected!=null)Selected(id);e.Handled=true;};System.Windows.Automation.AutomationProperties.SetName(z,Destination.Physical[id]);Canvas.SetLeft(z,x);Canvas.SetTop(z,y);z.Focusable=true;z.KeyDown+=(s,e)=>{if(e.Key==System.Windows.Input.Key.Enter||e.Key==System.Windows.Input.Key.Space){Select(id);if(Selected!=null)Selected(id);e.Handled=true;}};z.GotKeyboardFocus+=(s,e)=>Select(id);drawing.Children.Add(z);zones[id]=z;}
  public void Select(int id){selected=id;Refresh();}
  void Refresh(){foreach(var p in zones){p.Value.BorderBrush=p.Key==selected?accent:outline;p.Value.Background=pressed[p.Key]?accent:Idle(p.Key);((TextBlock)p.Value.Child).Foreground=pressed[p.Key]?Ui.Inset:Ui.Brush("#383B3D");}foreach(var p in sculpted){bool direction=p.Key>=13&&p.Key<=16;p.Value.Fill=pressed[p.Key]?accent:direction?Brushes.Transparent:Idle(p.Key);p.Value.Stroke=p.Key==selected?accent:direction?Brushes.Transparent:outline;if(labels.ContainsKey(p.Key))labels[p.Key].Foreground=pressed[p.Key]?Ui.Inset:Ui.Brush("#383B3D");if(directionMarks.ContainsKey(p.Key))directionMarks[p.Key].Stroke=pressed[p.Key]?Ui.Inset:outline;}}
  public void Update(PadStatus status){bool[] current=status==null?new bool[19]:status.Buttons;bool changed=false;for(int i=0;i<19;i++)if(current[i]!=pressed[i]){changed=true;break;}if(changed){pressed=current;Refresh();}for(int i=0;i<2;i++){byte travel=status==null?(byte)0:status.Raw[8+i];if(travel==0)sculpted[17+i].Fill=Idle(17+i);else if(travel==255)sculpted[17+i].Fill=accent;else {Color from=((SolidColorBrush)Idle(17+i)).Color,to=((SolidColorBrush)accent).Color;double amount=.18+.82*travel/255.0;triggerInk[i].Color=Color.FromRgb((byte)(from.R+(to.R-from.R)*amount),(byte)(from.G+(to.G-from.G)*amount),(byte)(from.B+(to.B-from.B)*amount));sculpted[17+i].Fill=triggerInk[i];}labels[17+i].Foreground=travel>100?Ui.Inset:Ui.Text;}Move(left,status,4);Move(right,status,6);}
  static void Move(TranslateTransform transform,PadStatus status,int offset){transform.X=status==null?0:Math.Max(-1,(status.Raw[offset]-128)/127.0)*13;transform.Y=status==null?0:Math.Max(-1,(status.Raw[offset+1]-128)/127.0)*13;}
  static double ColorDistance(Color a,Color b){return Math.Pow(a.R-b.R,2)+Math.Pow(a.G-b.G,2)+Math.Pow(a.B-b.B,2);}
  public void VerifyMotion(){PadStatus input=new PadStatus{HasInput=true};input.Raw[1]=1;input.Raw[2]=64;input.Raw[3]=68;input.Raw[8]=255;input.Raw[4]=255;input.Raw[5]=0;input.Raw[6]=0;input.Raw[7]=255;Update(input);if(left.X!=13||left.Y!=-13||right.X!=-13||right.Y!=13||zones[0].Background!=accent||sculpted[4].Fill!=accent||sculpted[17].Fill!=accent||sculpted[13].Fill!=accent||sculpted[16].Fill!=accent)throw new Exception("Controller motion, shoulder or diagonal D-pad feedback failed.");input.Raw[8]=1;Update(input);if(((SolidColorBrush)sculpted[17].Fill).Color==Ui.Color("#414747"))throw new Exception("Trigger must light at first nonzero travel.");input.Raw[8]=64;Update(input);Color early=((SolidColorBrush)sculpted[17].Fill).Color;input.Raw[8]=128;Update(input);if(ColorDistance(((SolidColorBrush)sculpted[17].Fill).Color,((SolidColorBrush)accent).Color)>=ColorDistance(early,((SolidColorBrush)accent).Color))throw new Exception("Trigger intensity must increase with travel.");Select(14);if(sculpted[14].Stroke!=accent)throw new Exception("D-pad selection failed.");Update(null);if(left.X!=0||left.Y!=0||right.X!=0||right.Y!=0||zones[0].Background!=idle||((SolidColorBrush)sculpted[4].Fill).Color!=Ui.Color("#B9C1BD")||sculpted[13].Fill!=Brushes.Transparent)throw new Exception("Controller did not reset after disconnect.");}
 }
}

