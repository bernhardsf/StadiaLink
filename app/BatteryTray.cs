using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using Forms=System.Windows.Forms;
namespace StadiaStudio {
 public static class Battery {
  [DllImport("StadiaDevice.dll",CallingConvention=CallingConvention.Winapi)] static extern int StadiaReadBattery(ulong address,out int level);
  public static int? Read(ulong address){int level;int hr=StadiaReadBattery(address,out level);return hr>=0&&level>=0&&level<=100?(int?)level:null;}
 }
 public sealed class BatteryTray:IDisposable {
  readonly Forms.NotifyIcon tray=new Forms.NotifyIcon();readonly Icon logo;Icon batteryIcon;
  readonly Forms.ToolStripMenuItem level=new Forms.ToolStripMenuItem("Batería: —"){Enabled=false};int? lastLevel;bool lastConnected;bool lastCharging;bool first=true;
  readonly Queue<DeviceNotice> pending=new Queue<DeviceNotice>();readonly Forms.Timer noticeTimer=new Forms.Timer{Interval=6500};
  string lastLanguage;bool updateNotice;public Action UpdateClicked;
  [DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr icon);
  public BatteryTray(Action open,Action exit){
   using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("app.ico"))using(var embedded=new Icon(stream,32,32))logo=(Icon)embedded.Clone();tray.Icon=logo;tray.Text=Brand.Title;
   var menu=new Forms.ContextMenuStrip();menu.BackColor=Color.FromArgb(26,34,37);menu.ForeColor=Color.FromArgb(233,238,235);menu.Items.Add(level);menu.Items.Add(new Forms.ToolStripSeparator());menu.Items.Add(I18n.T("Abrir StadiaLink"),null,(s,e)=>open()).Tag="Abrir StadiaLink";menu.Items.Add(I18n.T("Salir"),null,(s,e)=>exit()).Tag="Salir";tray.ContextMenuStrip=menu;tray.BalloonTipClicked+=(s,e)=>{if(updateNotice&&UpdateClicked!=null)UpdateClicked();else open();};tray.MouseClick+=(s,e)=>{if(e.Button==Forms.MouseButtons.Left)open();};tray.Visible=true;
   noticeTimer.Tick+=(s,e)=>{if(pending.Count==0)noticeTimer.Stop();else ShowNotice(pending.Dequeue());};
  }
  public bool Visible {get{return tray.Visible;}}
  public void Update(bool connected,int? percent,bool charging=false){
   if(!first && lastLevel==percent && lastConnected==connected&&lastCharging==charging&&lastLanguage==I18n.Code)return;first=false;lastLevel=percent;lastConnected=connected;lastCharging=charging;lastLanguage=I18n.Code;foreach(Forms.ToolStripItem item in tray.ContextMenuStrip.Items)if(item.Tag is string)item.Text=I18n.T((string)item.Tag);
   level.Text=I18n.T(!connected?"Mando desconectado":percent.HasValue?"Batería: "+percent.Value+" %":"Batería: no disponible");
   tray.Text=I18n.T(!connected?"StadiaLink · sin mando":percent.HasValue?"Stadia · "+percent.Value+" %":"StadiaLink · batería no disponible");
   if(connected&&charging){string suffix=I18n.T(percent==100?" · Carga completa":" · Cargando");level.Text+=suffix;tray.Text=(tray.Text+suffix).Substring(0,Math.Min(63,tray.Text.Length+suffix.Length));}
   // Use a battery silhouette for live status; keep branding in the taskbar.
   Icon next=null;
   if(connected&&percent.HasValue)using(var bitmap=DrawBadge(percent.Value,charging)){
    IntPtr handle=bitmap.GetHicon();try{using(var borrowed=Icon.FromHandle(handle))next=(Icon)borrowed.Clone();}finally{DestroyIcon(handle);}
   }
   tray.Icon=next??logo;Icon previous=batteryIcon;batteryIcon=next;if(previous!=null)previous.Dispose();
  }
  void ShowNotice(DeviceNotice notice){updateNotice=notice.Title=="Actualización disponible";tray.ShowBalloonTip(5000,I18n.T(notice.Title),I18n.T(notice.Text),notice.Warning?Forms.ToolTipIcon.Warning:Forms.ToolTipIcon.Info);}
  public void Notify(DeviceNotice notice){if(noticeTimer.Enabled){if(pending.Count<8)pending.Enqueue(notice);}else{ShowNotice(notice);noticeTimer.Start();}}
  public void ClearNotifications(){pending.Clear();noticeTimer.Stop();}
  public static Bitmap DrawBadge(int percent){return DrawBadge(percent,false);}
  public static Bitmap DrawBadge(int percent,bool charging){
   percent=Math.Max(0,Math.Min(100,percent));var bitmap=new Bitmap(16,16);
   using(var g=Graphics.FromImage(bitmap))using(var outline=new SolidBrush(Color.FromArgb(240,244,242)))using(var background=new SolidBrush(Color.FromArgb(35,40,43)))using(var fill=new SolidBrush(percent<=15?Color.FromArgb(255,117,107):Color.FromArgb(181,232,121))){
    g.Clear(Color.Transparent);
    g.FillRectangle(outline,1,3,12,10);g.FillRectangle(outline,13,6,2,4);
    g.FillRectangle(background,2,4,10,8);
    int width=(int)Math.Round(8*percent/100.0);if(percent>0)width=Math.Max(1,width);
    if(width>0)g.FillRectangle(fill,3,5,width,6);
    if(charging){var bolt=new[]{new Point(8,3),new Point(5,8),new Point(7,8),new Point(6,13),new Point(10,7),new Point(8,7)};g.FillPolygon(outline,bolt);}
   }
   return bitmap;
  }
  public void Dispose(){noticeTimer.Dispose();tray.Visible=false;tray.Dispose();if(batteryIcon!=null)batteryIcon.Dispose();if(logo!=null)logo.Dispose();}
 }
}


