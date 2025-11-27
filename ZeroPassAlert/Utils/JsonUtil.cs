using System.Text;

namespace ZeroPassAlert.Utils
{
    public static class JsonUtil
    {
        public static string ConvertCamelToSnake(string camelCase)
        {
            StringBuilder snakeCase = new StringBuilder();
            for (int i = 0; i < camelCase.Length; i++)
            {
                char currentChar = camelCase[i];
                if (char.IsUpper(currentChar) && i > 0)
                {
                    snakeCase.Append('_');
                }
                snakeCase.Append(char.ToLower(currentChar));
            }
            return snakeCase.ToString();
        }

        public static string ConvertSnakeToCamel(string snakeCase)
        {
            StringBuilder camelCase = new StringBuilder();
            bool capitalizeNext = false;

            foreach (char c in snakeCase)
            {
                if (c == '_')
                {
                    capitalizeNext = true;
                }
                else if (capitalizeNext)
                {
                    camelCase.Append(char.ToUpper(c));
                    capitalizeNext = false;
                }
                else
                {
                    camelCase.Append(char.ToLower(c));
                }
            }

            return camelCase.ToString();
        }
    }
}