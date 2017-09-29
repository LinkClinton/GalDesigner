﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public class ConfigAnalyser : ResourceAnalyser
    {
        enum ValueType
        {
            Unknown,
            Int,
            Bool,
            Float,
            String
        }

        private class Sentence
        {
            public ValueType Type = ValueType.Unknown;
            public object Value = null;
            public string ValueName = null;

            public static string Include(string input) => '"' + input + '"';

            public static string Unclude(string input) => input.Substring(1, input.Length - 2);

            public static ValueType GetType(object value)
            {
                switch (value)
                {
                    case string String:
                        return ValueType.String;

                    case int Int:
                        return ValueType.Int;

                    case bool Bool:
                        return ValueType.Bool;

                    case float Float:
                        return ValueType.Float;

                    default:
                        return ValueType.Unknown;
                }
            }

            public static ValueType GetType(string value, int Line, string FileTag)
            {
                //Test String
                if (value[0] is '"' && value[value.Length - 1] is '"') return ValueType.String;

                //Test Bool
                if (value is "true" || value is "false") return ValueType.Bool;

                int PointCount = 0;

                //Test int and float
                foreach (var item in value)
                {
                    if (item is '.')
                    {
                        PointCount++;

                        if (PointCount > 1) DebugLayer.ReportError(ErrorType.InconsistentResourceParameters, Line, FileTag);

                        continue;
                    }

                    if (Utilities.IsNumber(item) is false) DebugLayer.ReportError(ErrorType.InconsistentResourceParameters, Line, FileTag);
                }

                if (value.Contains('.')) return ValueType.Float;
                else return ValueType.Int;
            }

            public override string ToString()
            {
                switch (Type)
                {
                    case ValueType.Unknown:
                        return "";
                        
                    case ValueType.Int:
                        return ValueName + " = " + (int)Value;
                        
                    case ValueType.Bool:
                        string tempValue = null;

                        if (Value is true) tempValue = "true";
                        else tempValue = "false";

                        return ValueName + " = " + tempValue; 

                    case ValueType.Float:
                        return ValueName + " = " + (float)Value;
                        
                    case ValueType.String:
                        return ValueName + " = " + Include(Value as string);
                        
                    default:
                        return "";
                }
            }
        }

        private Dictionary<string, object> valueList;

        private void ProcessSentenceValue(ref string value, int Line, string FileTag)
        {
            Sentence sentence = new Sentence();

            var result = value.Split(new char[] { '=' }, 2);

            var left = result[0]; var right = result[1];

            sentence.ValueName = left;
            sentence.Type = Sentence.GetType(right, Line, FileTag);

            switch (sentence.Type)
            {
                case ValueType.Int:
                    sentence.Value = Convert.ToInt32(right);
                    break;

                case ValueType.Bool:
                    if (right is "true") sentence.Value = true;
                    else sentence.Value = false;
                    break;

                case ValueType.Float:
                    sentence.Value = (float)Convert.ToDouble(right);
                    break;

                case ValueType.String:
                    sentence.Value = Sentence.Unclude(right);
                    break;

                default:
                    break;
            }

            valueList[sentence.ValueName] = sentence.Value;

            GlobalConfig.SetValue(sentence.ValueName, sentence.Value);

            value = "";
        }

        internal ConfigAnalyser(string Tag, string FilePath) : base(Tag, FilePath)
        {
            valueList = new Dictionary<string, object>();   
        }

        protected override void ProcessReadFile(ref string contents)
        {
            string currentString = "";

            bool inCodeBlock = false;
            bool inString = false;

            int line = 0;

            foreach (var item in contents)
            {
                if (item is '\n') { line++; continue; }

                if (item is '"') { currentString += item; inString ^= true; continue; }

                //Bulid String to making Sentence
                if (Utilities.IsAlphaOrNumber(item) is true || item is '=' || inString is true)
                {
                    currentString += item;
                    continue;
                }

                if (item is '{')
                {
#if DEBUG
                    DebugLayer.Assert(inCodeBlock is true, ErrorType.InvalidResourceFormat, line, Tag);
#endif

                    inCodeBlock = true;
                    continue;
                }

                //Find Block
                if (item is '}')
                {
#if DEBUG
                    DebugLayer.Assert(inCodeBlock is false, ErrorType.InvalidResourceFormat, line, Tag);
#endif

                    inCodeBlock = false;

                    ProcessSentenceValue(ref currentString, line, Tag); continue;
                }

                if (item is ',')
                {
                    ProcessSentenceValue(ref currentString, line, Tag); continue;
                }

            }

            if (inCodeBlock is true || inString is true)
                DebugLayer.ReportError(ErrorType.InvalidResourceFormat, line, Tag);
        }

        protected override void ProcessWriteFile(out string contents)
        {
            contents = "{\n";

            int line = 0;

            foreach (var item in valueList)
            {
                line++;

                Sentence sentence = new Sentence()
                {
                    Value = GlobalConfig.GetValue(item.Key),
                    ValueName = item.Key,
                    Type = Sentence.GetType(item.Value)
                };

                contents += "\t" + sentence;

                if (line < valueList.Count) contents += ',';

                contents += "\n";
            }

            contents += "}\n";
        }
    }
}