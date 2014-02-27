using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpMule.Automation.Framework.Essentials
{
    [Serializable]
    public class Command
    {
        //vars
        string proc;
        string param;
        string desc;
        bool skip;
        bool ignore;

        // Constructors
        public Command()
        {
            this.proc = String.Empty;
            this.param = String.Empty;
            this.desc = String.Empty;
        }
        public Command(string procedure, string parameters, string description, bool ignore, bool skip)
        {
            this.proc = procedure;
            this.param = parameters;
            this.desc = description;
            this.ignore = ignore;
            this.skip = skip;
        }

        //Properties
        public string Proc
        {
            get { return proc; }
            set { proc = value; }
        }
        public string Param
        {
            get { return param; }
            set { param = value; }
        }
        public string Desc
        {
            get { return desc; }
            set { desc = value; }
        }

        public bool Skip
        {
            get { return skip; }
            set { skip = value; }
        }


        public bool Ignore
        {
            get { return ignore; }
            set { ignore = value; }
        }
    }

}
