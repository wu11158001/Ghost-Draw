using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace GhostDrawServer.Tools
{
    class Utils
    {
        /// <summary>
        /// 判斷是否為數字或英文字母
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsAlphaNumeric(string str)
        {
            Regex regex = new Regex("^[a-zA-Z0-9]+$");
            return regex.IsMatch(str);
        }

        /// <summary>
        /// 字串加法
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static string StringAddition(string v1, string v2)
        {
            StringBuilder sb = new StringBuilder();
            Sum(v1.Length - 1, v2.Length - 1, false);
            return sb.ToString();

            // 相加
            void Sum(int index1, int index2, bool isCarry)
            {
                if (index1 < 0 && index2 < 0 && !isCarry)
                {
                    return;
                }

                int num1 = index1 >= 0 ? Convert.ToInt32(v1[index1].ToString()) : 0;
                int num2 = index2 >= 0 ? Convert.ToInt32(v2[index2].ToString()) : 0;

                int sum = num1 + num2 + (isCarry ? 1 : 0);

                sb.Insert(0, (sum % 10).ToString());

                Sum(index1 - 1, index2 - 1, sum / 10 >= 1);
            }
        }
    }
}
