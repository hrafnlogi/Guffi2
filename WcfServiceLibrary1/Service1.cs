using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;


namespace WcfServiceLibrary1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class Service1 : IService1
    {
        public string GetData(int value)
        {
            return string.Format("You enterede: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

        public void prufuFall(int value)
        {
            
        }

        public void breytaTexta(string texti)
        {
            TextWriter txt = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"\skilabodTexti.txt");
            txt.Write(texti);
            txt.Close();
        }

        public void breytaHelgarTexta(string texti)
        {
            TextWriter txt = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"\skilabodTextiHelgi.txt");
            txt.Write(texti);
            txt.Close();
        }


        public void breytaVirkurTexta(string texti)
        {
            TextWriter txt = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"\skilabodTextiVirkur.txt");
            txt.Write(texti);
            txt.Close();
        }


        public string faHelgarTexta()
        {

            string helgarTexti = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"skilabodTextiHelgi.txt");
            return helgarTexti;
        }

        public string faVirkurTexta()
        {
            string virkurTexti = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"skilabodTextiVirkur.txt");
            return virkurTexti;
        }

        public string faLog(string valingDagsetning)
        {

            string filepath = AppDomain.CurrentDomain.BaseDirectory + @"logs\" + valingDagsetning + ".txt";
            if (!File.Exists(filepath))
            {
                return "nofile";
            }
            else
            {
                string log = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"logs\" + valingDagsetning + ".txt");
                return log;
            }
            
        }

    }
}
