using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace SharpMule.Automation.Framework.Essentials
{
    public delegate void ExceptionDelegate();

    [Serializable]
    public static class CommandExceptions
    {
        
        public static Logger Log { get; set; }
        public static bool Try(ExceptionDelegate v)
        {
            try { v(); return true; }
            catch (WebException ex) { Log.LogDebug(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()); return false; }
            catch (Exception ex) { Log.LogDebug(ex.Message); return false; }
        }
        public static bool Try(ExceptionDelegate v, string error)
        {
            try { v(); return true; }
            catch (WebException ex) { Log.LogError(error); Log.LogDebug(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()); return false; }
            catch (Exception ex) { Log.LogError(error); Log.LogDebug(ex.Message); return false; }

        }


    }
}
