using System.Collections.Generic;

public struct User
{
    public User(int id, string account, string password)
    {
        this.id = id;
        this.account = account;
        this.password = password;
    }

    public int id { get; private set;}
    public string account { get; private set;}
    public string password { get; private set; }
}

public class UserCollection : List<User>
{
    public UserCollection() : base()
    {

    }
    public UserCollection(IEnumerable<User> users)
    {
        this.AddRange(users);
    }
}
