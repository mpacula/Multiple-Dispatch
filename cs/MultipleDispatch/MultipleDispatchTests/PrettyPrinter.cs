using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using MultipleDispatch;

namespace MultipleDispatchTests
{
    class PrettyPrinter
    {
        public string Print(string str)
        {
            return String.Format("STRING({0})", str);
        }

        public string Print(object val, string comment)
        {
            return Multimethod.Call<string>(this, "Print", val) + " // " + comment;
        }

        public string Print(float value)
        {
            return String.Format("FLOAT({0})", value);
        }

        public string Print(double value) 
        {
            return String.Format("DOUBLE({0})", value);
        }

        public string Print(int value)
        {
            return String.Format("INT({0})", value);
        }

        public string Print(object value)
        {
            return "???";
        }

        public string Print(IEnumerable collection)
        {
            string result = "[";

            bool first = true;
            foreach (var obj in collection)
            {
                if (!first)
                    result += ", ";

                result += Multimethod.Call<string>(this, "Print", obj);

                if (first) first = false;
            }

            return result + "]";
        }
    }
}
