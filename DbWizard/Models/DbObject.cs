using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbWizard.Models
{
    public class DbObject
    {
        private Dictionary<string, object> _dict = new Dictionary<string, object>();
        public DbObjectType Type { get; set; }
        public string Name { get; set; }
        public bool LoadCompleted { get; set; }

        public DbObject()
        {

        }

        public object this[string key]
        {
            get
            {
                if (_dict.ContainsKey(key))
                    return _dict[key];
                return null;
            }
            set
            {
                _dict[key] = value;
            }
        }
    }

    public enum DbObjectType
    {
        Database,
        Table,
        Column,
        StoredProcedure
    }
}
