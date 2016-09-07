using System.Text;

namespace SerialToKeyboard.Control
{
    class DefaultFormatter
    {
        public static string Reformat(string original)
        {
            var sb = new StringBuilder();
            foreach (char c in original)
            {
                switch (c)
                {
                    case '+': case '^': case '~': case '%':
                    case '(': case ')': case '[': case ']':
                        sb.AppendFormat("{{{0}}}", c);
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
