using JWT.Algorithms;
using JWT.Builder;
using MyConversation.Model.Model;
using MyConversation.Model.SystemModel;
using MyConversation.Repository.Repository;
using System.Text;

namespace MyConversation.Helper
{
    public class CommonHandler
    {
        public static BaseModel UpdateField(BaseModel input, string modifiedBy="")
        {
            input.ModifiedDate = DateTime.Now;
            input.ModifiedBy = modifiedBy;
            return input;
        }
        public static User HandleToken(string token)
        {
            try
            {
                token = token.Replace("Bearer ", string.Empty);
                var SecretKeyJWT = AppsettingConfig.GetByKey("SecretKeyJWT");
                var jwt = JwtBuilder.Create()
                        .WithAlgorithm(new HMACSHA512Algorithm()) // symmetric
                        .WithSecret(SecretKeyJWT)
                        .MustVerifySignature()
                        .Decode<IDictionary<string, object>>(token);
                using (var repUser = new UserRepository(CommonHandler.GeneralDB))
                {
                    var userResponse = repUser.Single(x => x.Username == jwt["username"].ToString() && x.Password == jwt["password"].ToString());
                    if (userResponse != null && userResponse.IsSuccess && userResponse.Data != null)
                    {
                        return userResponse.Data;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public static string GenerateID(string prefix)
        {
            string id = string.Empty;
            id = RemoveSpecialCharacters(RemoveSign4VietnameseString(prefix)) + "_" + DateTime.Now.ToFileTime();
            return id;
        }

        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_' || c == ' ')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static string RemoveSign4VietnameseString(string str)
        {
            for (int i = 1; i < VietnameseSigns.Length; i++)
            {
                for (int j = 0; j < VietnameseSigns[i].Length; j++)
                    str = str.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
            }
            return str;
        }
        private static readonly string[] VietnameseSigns = new string[]
        {

            "aAeEoOuUiIdDyY",

            "áàạảãâấầậẩẫăắằặẳẵ",

            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",

            "éèẹẻẽêếềệểễ",

            "ÉÈẸẺẼÊẾỀỆỂỄ",

            "óòọỏõôốồộổỗơớờợởỡ",

            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",

            "úùụủũưứừựửữ",

            "ÚÙỤỦŨƯỨỪỰỬỮ",

            "íìịỉĩ",

            "ÍÌỊỈĨ",

            "đ",

            "Đ",

            "ýỳỵỷỹ",

            "ÝỲỴỶỸ"
        };


        #region constants
        public static string GeneralDB = "GeneralDB";
        #endregion
    }
}
