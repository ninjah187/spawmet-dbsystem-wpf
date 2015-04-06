namespace SpawmetDatabase.Model
{
    public class User : IModelElement
    {
        public User()
        {
            
        }

        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public UserGroup Group { get; set; }
    }
}
