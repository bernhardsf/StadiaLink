// HTML window simulation. Native OS sizing/Snap remains production work.
let maximized=false,restoreBounds=null;
function availableWidth(){const style=getComputedStyle(document.body);return Math.max(240,document.body.clientWidth-parseFloat(style.paddingLeft||0)-parseFloat(style.paddingRight||0))}
function size(width){const win=document.getElementById('window');maximized=false;restoreBounds=null;win.style.width=Math.min(width,availableWidth())+'px';win.style.height='830px';updateCaption()}
function updateCaption(){const button=document.querySelector('[aria-label="Maximize or restore preview"]');button.title=maximized?'Restore preview':'Maximize preview';button.setAttribute('aria-pressed',String(maximized))}
function maximize(){const win=document.getElementById('window');if(!maximized){restoreBounds={width:win.style.width,height:win.style.height};win.style.width='100%';win.style.height='calc(100vh - 110px)'}else{win.style.width=restoreBounds?.width||'100%';win.style.height=restoreBounds?.height||'830px'}maximized=!maximized;updateCaption()}
function minimize(){document.getElementById('restore').textContent='Restore StadiaLink preview';clearTimeout(motorTimer);document.getElementById('window').hidden=true;const button=document.getElementById('restore');button.hidden=false;button.focus()}
function closePreview(){minimize()}
function restore(){document.getElementById('restore').textContent='Restore StadiaLink preview';document.getElementById('window').hidden=false;document.getElementById('restore').hidden=true;document.querySelector('.nav button.selected').focus()}
new ResizeObserver(entries=>{document.getElementById('dimensions').textContent=Math.round(entries[0].contentRect.width)+' px'}).observe(document.getElementById('window'));
