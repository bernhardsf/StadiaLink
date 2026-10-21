using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using Microsoft.Win32;

[assembly:AssemblyTitle("StadiaLink Setup (local)")]
[assembly:AssemblyProduct("StadiaLink")]
[assembly:AssemblyCompany("bernhardsf")]
[assembly:AssemblyVersion("1.0.4.0")]
[assembly:AssemblyFileVersion("1.0.4.0")]

namespace StadiaStudio {
    public static class SetupEntry {
#if UPDATE_ONLY
        public static readonly bool UpdateOnly=true;
#else
        public static readonly bool UpdateOnly=false;
#endif
        [STAThread] public static int Main(string[] args) {
            try {
                if(!(args.Length==2&&args[0]=="/languages-test"))I18n.Set("en");
                if(args.Length==2&&args[0]=="/update-test"){UpdatePackage.Verify(args[1]);return 0;}
                if(args.Length==2 && args[0]=="/extract") {Extract(args[1]);return 0;}
                Application application=new Application();if(args.Length==2&&args[0]=="/languages-test"){application.ShutdownMode=ShutdownMode.OnExplicitShutdown;Directory.CreateDirectory(args[1]);string code=I18n.Code;try{foreach(LanguageOption option in I18n.Options){I18n.Set(option.Code);foreach(bool remove in new[]{false,true}){SetupWindow check=new SetupWindow(remove);check.Show();check.UpdateLayout();check.Snapshot(Path.Combine(args[1],option.Code+(remove?"-uninstall":"-install")+".png"));check.Close();}}File.WriteAllText(Path.Combine(args[1],"result.txt"),"PASS: installer and uninstaller rendered in all four languages.");return 0;}finally{I18n.Set(code);application.Shutdown();}}SetupWindow window=new SetupWindow(args.Length==1&&args[0]=="/uninstall");
                if(args.Length==1&&args[0]=="/install-test") window.Loaded+=async(s,e)=>{try{await window.Install();}catch(Exception ex){Log(ex.ToString());Environment.ExitCode=1;}finally{window.Close();}};
                if(args.Length==2&&args[0]=="/smoke")window.Loaded+=(s,e)=>{DispatcherTimer t=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(500)};t.Tick+=(a,b)=>{t.Stop();window.Snapshot(args[1]);window.Close();};t.Start();};
                application.Run(window);return Environment.ExitCode;
            }catch(Exception ex){Log(ex.ToString());if(args.Length==0)MessageBox.Show(I18n.Error(ex),"StadiaLink");return 1;}
        }
        public static void Log(string text){string path=Path.Combine(ProfileLibrary.DirectoryPath,"logs");Directory.CreateDirectory(path);File.AppendAllText(Path.Combine(path,"setup.log"),DateTime.Now.ToString("s")+" "+text+Environment.NewLine);}
        public static void Extract(string directory) {
            byte[] bytes;using(Stream stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("payload.zip")) {if(stream==null)throw new InvalidDataException("No se encuentra el paquete de instalación.");using(MemoryStream m=new MemoryStream()){stream.CopyTo(m);bytes=m.ToArray();}}
            using(SHA256 hash=SHA256.Create()){string actual=BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-","");if(actual!=PayloadInfo.Sha256)throw new InvalidDataException("El paquete de instalación está dañado.");}
            string root=Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;Directory.CreateDirectory(root);
            using(MemoryStream m=new MemoryStream(bytes))using(ZipArchive archive=new ZipArchive(m,ZipArchiveMode.Read))foreach(ZipArchiveEntry entry in archive.Entries) {
                string path=Path.GetFullPath(Path.Combine(root,entry.FullName));if(!path.StartsWith(root,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Ruta no válida en el instalador.");
                if(String.IsNullOrEmpty(entry.Name)){Directory.CreateDirectory(path);continue;}Directory.CreateDirectory(Path.GetDirectoryName(path));using(Stream input=entry.Open())using(FileStream output=new FileStream(path,FileMode.Create,FileAccess.Write))input.CopyTo(output);
            }
        }
    }
    public sealed class SetupWindow:Window {
        readonly TextBlock state;readonly ProgressBar progress=new ProgressBar{Height=4,Maximum=100,Foreground=SetupTheme.Lime,Background=SetupTheme.Line,BorderThickness=new Thickness(0)};
        readonly Button install;readonly CheckBox removeDriver=new CheckBox{Content="Retirar también el controlador del mando",IsChecked=false};readonly bool uninstall;bool working,complete;
        public SetupWindow(bool remove) {
            state=SetupTheme.T(remove?"Todo preparado para desinstalar.":SetupEntry.UpdateOnly?"Todo preparado para actualizar.":"Todo preparado para instalar.",13,SetupTheme.Muted);
            uninstall=remove;Title=Brand.Title;Width=700;Height=740;MinWidth=560;MinHeight=600;ResizeMode=ResizeMode.CanResize;WindowStartupLocation=WindowStartupLocation.CenterScreen;Background=SetupTheme.Background;Foreground=SetupTheme.Text;FontFamily=new FontFamily("Segoe UI");WindowStyle=WindowStyle.SingleBorderWindow;
            Ui.Styles(Resources,true);
            Grid layout=new Grid{Margin=new Thickness(38,22,38,32)};layout.RowDefinitions.Add(new RowDefinition{Height=new GridLength(50)});layout.RowDefinitions.Add(new RowDefinition());layout.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});
            TextBlock brand=SetupTheme.T(Brand.Stamp,12,SetupTheme.Muted);brand.VerticalAlignment=VerticalAlignment.Center;layout.Children.Add(brand);
            StackPanel body=new StackPanel();body.Children.Add(SetupTheme.Logo(72));body.Children.Add(SetupTheme.Gap(12));body.Children.Add(SetupTheme.T(remove?"Desinstalar StadiaLink":SetupEntry.UpdateOnly?"Actualizar StadiaLink":"Instalar StadiaLink",30,null,true));body.Children.Add(SetupTheme.Gap(12));body.Children.Add(SetupTheme.T(remove?"Retira la aplicación de esta cuenta de Windows. Tus perfiles se conservarán.":SetupEntry.UpdateOnly?"Actualiza la aplicación instalada y conserva tus perfiles.":"Aplicación y controlador para Windows 11 x64.",14,SetupTheme.Muted));body.Children.Add(SetupTheme.Gap(18));
            StackPanel benefits=new StackPanel();benefits.Children.Add(SetupTheme.T(remove?"DESINSTALACIÓN":"INCLUYE",10,SetupTheme.Lime,true));benefits.Children.Add(SetupTheme.Gap(15));
            if(remove){benefits.Children.Add(SetupTheme.T("Se eliminarán StadiaLink y su acceso del menú Inicio.",13));benefits.Children.Add(removeDriver);benefits.Children.Add(SetupTheme.Gap(10));benefits.Children.Add(SetupTheme.T("Retirar el controlador afecta a todos los usuarios del equipo y requiere permiso de administrador.",12,SetupTheme.Muted));}
            else{benefits.Children.Add(SetupTheme.T("✓  Detección automática por Bluetooth y USB",14));benefits.Children.Add(SetupTheme.Gap(12));benefits.Children.Add(SetupTheme.T("✓  Botones, sticks, gatillos y vibración",14));benefits.Children.Add(SetupTheme.Gap(12));benefits.Children.Add(SetupTheme.T("✓  Perfiles y acciones de Windows",14));benefits.Children.Add(SetupTheme.Gap(12));benefits.Children.Add(SetupTheme.T("✓  Instalación sin descargas",14));}
            body.Children.Add(SetupTheme.Card(benefits,22));body.Children.Add(SetupTheme.Gap(20));body.Children.Add(SetupTheme.T(Brand.About,12,SetupTheme.Muted));ScrollViewer scroll=new ScrollViewer{Content=body,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled,Margin=new Thickness(0,0,0,24)};Grid.SetRow(scroll,1);layout.Children.Add(scroll);
            StackPanel bottom=new StackPanel();bottom.Children.Add(state);bottom.Children.Add(SetupTheme.Gap(12));bottom.Children.Add(progress);bottom.Children.Add(SetupTheme.Gap(18));Grid row=new Grid();row.ColumnDefinitions.Add(new ColumnDefinition());row.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});Button license=SetupTheme.Button("Licencia GPLv3",ShowLicense);license.BorderThickness=new Thickness(0);license.Background=SetupTheme.Background;license.HorizontalAlignment=HorizontalAlignment.Left;row.Children.Add(license);
            install=SetupTheme.Button(remove?"Desinstalar":SetupEntry.UpdateOnly?"Actualizar StadiaLink":"Instalar StadiaLink",async()=>{if(complete){if(!uninstall)Process.Start(Path.Combine(InstallerActions.InstallDirectory,"StadiaLink.exe"));Close();return;}try{await Install();}catch(Exception e){I18n.SetText(state,I18n.Error(e));state.Foreground=SetupTheme.Coral;SetupEntry.Log(e.ToString());install.Content=I18n.T("Reintentar");}},true);Grid.SetColumn(install,1);row.Children.Add(install);bottom.Children.Add(row);Grid.SetRow(bottom,2);layout.Children.Add(bottom);Content=layout;I18n.TranslateTree(layout);Closing+=(s,e)=>{if(working)e.Cancel=true;};
        }
        void CheckRequirements(){if(!Environment.Is64BitOperatingSystem)throw new InvalidOperationException("Se necesita Windows x64.");using(RegistryKey key=Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion")){int build;if(key==null||!Int32.TryParse(Convert.ToString(key.GetValue("CurrentBuildNumber")),out build)||build<22000)throw new InvalidOperationException("Se necesita Windows 11.");}}
        public async Task Install() {
            working=true;install.IsEnabled=false;removeDriver.IsEnabled=false;state.Foreground=SetupTheme.Muted;try {
                CheckRequirements();string root=InstallerActions.InstallDirectory;
                if(Process.GetProcessesByName("StadiaStudio").Length>0||Process.GetProcessesByName("StadiaLink").Length>0)throw new InvalidOperationException("Cierra StadiaLink antes de continuar.");
                if(uninstall){
                    state.Text=I18n.T("Retirando StadiaLink…");
                    if(removeDriver.IsChecked==true){state.Text=I18n.T("Retirando el controlador…");await InstallerActions.RunDriver(Path.Combine(root,"driver","install.ps1"),true);}
                    InstallerActions.UnregisterApp();progress.Value=80;
                    // The running uninstaller stays until it exits. The next
                    // installation replaces it; other application files go now.
                    string expected=Path.GetFullPath(root).TrimEnd('\\')+"\\";
                    if(!Directory.Exists(root))throw new DirectoryNotFoundException("La aplicación ya no está instalada.");
                    string running=Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
                    await Task.Run(()=>{foreach(string file in Directory.GetFiles(root,"*",SearchOption.AllDirectories)){string full=Path.GetFullPath(file);if(!full.StartsWith(expected,StringComparison.OrdinalIgnoreCase))throw new IOException("Ruta de desinstalación no válida.");if(full.Equals(running,StringComparison.OrdinalIgnoreCase))continue;File.Delete(full);}});
                    state.Text=I18n.T("Aplicación desinstalada. Tus perfiles se han conservado.");install.Content=I18n.T("Cerrar");
                }else if(SetupEntry.UpdateOnly){
                    state.Text=I18n.T("Preparando la aplicación…");progress.Value=25;
                    await Task.Run(()=>UpdatePackage.Apply(root));
                    state.Text=I18n.T("Aplicación y perfiles conservados. El controlador no se modifica.");install.Content=I18n.T("Abrir StadiaLink");
                }else {
                    state.Text=I18n.T("Preparando la aplicación…");progress.Value=15;await Task.Run(()=>SetupEntry.Extract(root));
                    string running=Assembly.GetExecutingAssembly().Location,target=Path.Combine(root,"StadiaLink.Setup.exe");if(!Path.GetFullPath(running).Equals(Path.GetFullPath(target),StringComparison.OrdinalIgnoreCase))File.Copy(running,target,true);
                    progress.Value=40;state.Text=I18n.T("Instalando el controlador. Acepta el permiso de Windows…");await InstallerActions.RunDriver(Path.Combine(root,"driver","install.ps1"),false);
                    progress.Value=80;state.Text=I18n.T("Creando el acceso del menú Inicio…");InstallerActions.RegisterApp();
                    string log=Path.Combine(ProfileLibrary.DirectoryPath,"logs","driver-install.log");bool restart=File.Exists(log)&&File.ReadAllText(log).Contains("pnputil exit code: 3010");
                    state.Text=I18n.T(restart?"Instalado. Si el mando no aparece, reconéctalo o reinicia Windows.":"Instalado. Enciende tu Stadia y abre StadiaLink.");install.Content=I18n.T("Abrir StadiaLink");
                }
                progress.Value=100;state.Foreground=SetupTheme.Success;complete=true;SetupEntry.Log("Installation operation completed: "+(uninstall?"uninstall":"install"));
            }finally{working=false;install.IsEnabled=true;removeDriver.IsEnabled=!complete;}
        }
        void ShowLicense(){using(Stream stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("LICENSE")){string text;using(StreamReader reader=new StreamReader(stream))text=reader.ReadToEnd();Window dialog=new Window{Title=I18n.T("Licencia GPLv3"),Width=750,Height=550,Owner=this,WindowStartupLocation=WindowStartupLocation.CenterOwner,Background=SetupTheme.Background};dialog.Content=new TextBox{Text=text,IsReadOnly=true,TextWrapping=TextWrapping.Wrap,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,Margin=new Thickness(20)};dialog.ShowDialog();}}
        public void Snapshot(string path){UpdateLayout();var image=new System.Windows.Media.Imaging.RenderTargetBitmap((int)ActualWidth,(int)ActualHeight,96,96,PixelFormats.Pbgra32);image.Render(this);var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));using(FileStream f=File.Create(path))encoder.Save(f);}
    }
    // Installer-only palette, leaving application settings and behavior intact.
    static class SetupTheme {
        public static readonly Brush Background=Ui.Brush("#202020"),Text=Ui.Brush("#F5F5F5"),Muted=Ui.Brush("#B5B5B5"),Line=Ui.Brush("#454545"),Lime=Ui.Brush("#60CDFF"),Coral=Ui.Brush("#FF99A4"),Success=Ui.Brush("#6CCB5F");
        public static TextBlock T(string text,double size,Brush color=null,bool bold=false){return Ui.T(text,size,color??Text,bold);}
        public static FrameworkElement Gap(int n){return Ui.Gap(n);}
        public static FrameworkElement Logo(double n){return Ui.Logo(n);}
        public static Border Card(UIElement child,int padding){return new Border{Child=child,Padding=new Thickness(padding),Background=Ui.Brush("#292929"),BorderBrush=Ui.Brush("#383838"),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(8)};}
        public static Button Button(string text,Action action,bool accent=false){Button b=Ui.Button(text,action,accent);b.Background=accent?Lime:Ui.Brush("#343434");b.Foreground=accent?Ui.Brush("#003047"):Text;b.BorderBrush=accent?Lime:Line;return b;}
    }

}




