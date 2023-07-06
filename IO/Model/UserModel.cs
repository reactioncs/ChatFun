namespace ChatCommon.Model
{
    public class UserModel
    {
        public string UserName { get; set; }
        public Guid UID { get; set; }

        public UserModel(string userName)
        {
            UserName = userName;
            UID = Guid.NewGuid();
        }

        public UserModel(string userName, Guid uid)
        {
            UserName = userName;
            UID = uid;
        }
    }
}
