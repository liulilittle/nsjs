namespace nsjsdotnet.Core.Utilits
{
    using nsjsdotnet.Core.Linq;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public static class Attributes
    {
        public static IList<T> GetAttributes<T>(this MemberInfo member)
            where T : Attribute
        {
            if (member == null)
            {
                throw new ArgumentNullException();
            }
            object[] attris = member.GetCustomAttributes(typeof(T), true);
            if (attris.IsNullOrEmpty())
            {
                return null;
            }
            IList<T> attributes = new List<T>();
            foreach (object attri in attris)
            {
                if (attri is T)
                {
                    attributes.Add((T)attri);
                }
            }
            return attributes;
        }

        public static T GetAttribute<T>(this MemberInfo member) where T : Attribute
        {
            Attribute attr = Attributes.GetAttribute(member, typeof(T));
            if (attr == null)
            {
                return null;
            }
            return (T)attr;
        }

        public static Attribute GetAttribute(this MemberInfo member, Type attribute)
        {
            if (attribute == null || member == null)
            {
                throw new ArgumentNullException();
            }
            if (!typeof(Attribute).IsAssignableFrom(attribute))
            {
                return null;
            }
            object[] attris = member.GetCustomAttributes(attribute, true);
            if (attris.IsNullOrEmpty())
            {
                return null;
            }
            return (Attribute)attris[0];
        }
    }
}
