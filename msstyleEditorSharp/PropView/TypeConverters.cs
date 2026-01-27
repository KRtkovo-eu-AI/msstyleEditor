using libmsstyle;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Globalization;
using System.Reflection;


namespace msstyleEditor.PropView
{
    public class StyleEnumConverter : TypeConverter
    {
        private List<VisualStyleEnumEntry> m_enumInfo;

        public StyleEnumConverter(List<VisualStyleEnumEntry> enumInfo)
        {
            m_enumInfo = enumInfo;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string s)
            {
                return m_enumInfo.Find((e) => e.Name == s).Value;
            }
            else return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is int i)
            {
                return m_enumInfo[i].Name;
            }
            else return value.ToString();
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<int> lst = new List<int>();
            foreach (var enm in m_enumInfo)
            {
                lst.Add(enm.Value);
            }
            return new StandardValuesCollection(lst);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }

    public class StyleStringResourceConverter : TypeConverter
    {
        private Dictionary<int, string> m_enumInfo;

        public StyleStringResourceConverter(Dictionary<int, string> enumInfo)
        {
            m_enumInfo = enumInfo;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string s)
            {
                var found = -1;
                Int32.TryParse(s.Substring(0, s.IndexOf(" - ")), out found);
                foreach (var kvp in m_enumInfo)
                {
                    if (kvp.Key == found) return kvp.Key;
                }
                return $"{found} - INVALID";
            }
            else return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is int i)
            {
                string result;
                if (m_enumInfo.TryGetValue(i, out result))
                {
                    return $"{i} - {result}";
                }
                else return value.ToString();
            }
            else return value.ToString();
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<int> lst = new List<int>();
            foreach (var enm in m_enumInfo)
            {
                lst.Add(enm.Key);
            }
            return new StandardValuesCollection(lst);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }

    public class NoStandardColorConverter : ColorConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return false;
        }

        // Like the original, but without "known", "named" or "system" colors.
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            if (value is Color)
            {
                if (destinationType == typeof(string))
                {
                    Color color = (Color)value;
                    if (color == Color.Empty)
                    {
                        return string.Empty;
                    }

                    if (culture == null)
                    {
                        culture = CultureInfo.CurrentCulture;
                    }

                    string separator = culture.TextInfo.ListSeparator + " ";
                    TypeConverter converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(int));
                    int num = 0;
                    string[] array;
                    if (color.A < byte.MaxValue)
                    {
                        array = new string[4];
                        array[num++] = converter.ConvertToString(context, culture, color.A);
                    }
                    else
                    {
                        array = new string[3];
                    }

                    array[num++] = converter.ConvertToString(context, culture, color.R);
                    array[num++] = converter.ConvertToString(context, culture, color.G);
                    array[num++] = converter.ConvertToString(context, culture, color.B);
                    return string.Join(separator, array);
                }

                if (destinationType == typeof(InstanceDescriptor))
                {
                    MemberInfo memberInfo = null;
                    object[] arguments = null;
                    Color color = (Color)value;
                    if (color.IsEmpty)
                    {
                        memberInfo = typeof(Color).GetField("Empty");
                    }
                    else if (color.A != byte.MaxValue)
                    {
                        memberInfo = typeof(Color).GetMethod("FromArgb", new Type[4]
                        {
                            typeof(int),
                            typeof(int),
                            typeof(int),
                            typeof(int)
                        });
                        arguments = new object[4] { color.A, color.R, color.G, color.B };
                    }
                    else
                    {
                        memberInfo = typeof(Color).GetMethod("FromArgb", new Type[3]
                        {
                            typeof(int),
                            typeof(int),
                            typeof(int)
                        });
                        arguments = new object[3] { color.R, color.G, color.B };
                    }

                    if (memberInfo != null)
                    {
                        return new InstanceDescriptor(memberInfo, arguments);
                    }

                    return null;
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}