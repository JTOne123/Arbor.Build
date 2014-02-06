using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Mono.Options
{
    public abstract class Option {
        readonly string _prototype;
        readonly string _description;
        readonly string[] _names;
        readonly OptionValueType _type;
        readonly int _count;
        string[] _separators;
        readonly bool _hidden;
        
        protected Option (string prototype, string description, int maxValueCount = 1, bool hidden = false)
        {
            if (prototype == null)
                throw new ArgumentNullException ("prototype");
            if (prototype.Length == 0)
                throw new ArgumentException ("Cannot be the empty string.", "prototype");
            if (maxValueCount < 0)
                throw new ArgumentOutOfRangeException ("maxValueCount");

            this._prototype   = prototype;
            this._description = description;
            this._count       = maxValueCount;
            this._names       = (this is OptionSet.Category)
                // append GetHashCode() so that "duplicate" categories have distinct
                // names, e.g. adding multiple "" categories should be valid.
                ? new[]{prototype + this.GetHashCode ()}
                : prototype.Split ('|');

            if (this is OptionSet.Category)
                return;

            this._type        = ParsePrototype ();
            this._hidden      = hidden;

            if (this._count == 0 && _type != OptionValueType.None)
                throw new ArgumentException (
                    "Cannot provide maxValueCount of 0 for OptionValueType.Required or " +
                    "OptionValueType.Optional.",
                    "maxValueCount");
            if (this._type == OptionValueType.None && maxValueCount > 1)
                throw new ArgumentException (
                    string.Format ("Cannot provide maxValueCount of {0} for OptionValueType.None.", maxValueCount),
                    "maxValueCount");
            if (Array.IndexOf (_names, "<>") >= 0 && 
                ((_names.Length == 1 && this._type != OptionValueType.None) ||
                 (_names.Length > 1 && this.MaxValueCount > 1)))
                throw new ArgumentException (
                    "The default option handler '<>' cannot require values.",
                    "prototype");
        }

        public string           Prototype       {get {return _prototype;}}
        public string           Description     {get {return _description;}}
        public OptionValueType  OptionValueType {get {return _type;}}
        public int              MaxValueCount   {get {return _count;}}
        public bool             Hidden          {get {return _hidden;}}

        public string[] GetNames ()
        {
            return (string[]) _names.Clone ();
        }

        public string[] GetValueSeparators ()
        {
            if (_separators == null)
                return new string [0];
            return (string[]) _separators.Clone ();
        }

        protected static T Parse<T> (string value, OptionContext c)
        {
            Type tt = typeof (T);
            bool nullable = tt.IsValueType && tt.IsGenericType && 
                            !tt.IsGenericTypeDefinition && 
                            tt.GetGenericTypeDefinition () == typeof (Nullable<>);
            Type targetType = nullable ? tt.GetGenericArguments () [0] : typeof (T);
            TypeConverter conv = TypeDescriptor.GetConverter (targetType);
            T t = default (T);
            try {
                if (value != null)
                    t = (T) conv.ConvertFromString (value);
            }
            catch (Exception e) {
                throw new OptionException (
                    string.Format (
                        c.OptionSet.MessageLocalizer ("Could not convert string `{0}' to type {1} for option `{2}'."),
                        value, targetType.Name, c.OptionName),
                    c.OptionName, e);
            }
            return t;
        }

        internal string[] Names           {get {return _names;}}
        internal string[] ValueSeparators {get {return _separators;}}

        static readonly char[] NameTerminator = new char[]{'=', ':'};

        private OptionValueType ParsePrototype ()
        {
            char type = '\0';
            List<string> seps = new List<string> ();
            for (int i = 0; i < _names.Length; ++i) {
                string name = _names [i];
                if (name.Length == 0)
                    throw new ArgumentException ("Empty option names are not supported.", "prototype");

                int end = name.IndexOfAny (NameTerminator);
                if (end == -1)
                    continue;
                _names [i] = name.Substring (0, end);
                if (type == '\0' || type == name [end])
                    type = name [end];
                else 
                    throw new ArgumentException (
                        string.Format ("Conflicting option types: '{0}' vs. '{1}'.", type, name [end]),
                        "prototype");
                AddSeparators (name, end, seps);
            }

            if (type == '\0')
                return OptionValueType.None;

            if (_count <= 1 && seps.Count != 0)
                throw new ArgumentException (
                    string.Format ("Cannot provide key/value separators for Options taking {0} value(s).", _count),
                    "prototype");
            if (_count > 1) {
                if (seps.Count == 0)
                    this._separators = new[]{":", "="};
                else if (seps.Count == 1 && seps [0].Length == 0)
                    this._separators = null;
                else
                    this._separators = seps.ToArray ();
            }

            return type == '=' ? OptionValueType.Required : OptionValueType.Optional;
        }

        private static void AddSeparators (string name, int end, ICollection<string> seps)
        {
            int start = -1;
            for (int i = end+1; i < name.Length; ++i) {
                switch (name [i]) {
                    case '{':
                        if (start != -1)
                            throw new ArgumentException (
                                string.Format ("Ill-formed name/value separator found in \"{0}\".", name),
                                "prototype");
                        start = i+1;
                        break;
                    case '}':
                        if (start == -1)
                            throw new ArgumentException (
                                string.Format ("Ill-formed name/value separator found in \"{0}\".", name),
                                "prototype");
                        seps.Add (name.Substring (start, i-start));
                        start = -1;
                        break;
                    default:
                        if (start == -1)
                            seps.Add (name [i].ToString ());
                        break;
                }
            }
            if (start != -1)
                throw new ArgumentException (
                    string.Format ("Ill-formed name/value separator found in \"{0}\".", name),
                    "prototype");
        }

        public void Invoke (OptionContext c)
        {
            OnParseComplete (c);
            c.OptionName  = null;
            c.Option      = null;
            c.OptionValues.Clear ();
        }

        protected abstract void OnParseComplete (OptionContext c);

        public override string ToString ()
        {
            return Prototype;
        }
    }
}