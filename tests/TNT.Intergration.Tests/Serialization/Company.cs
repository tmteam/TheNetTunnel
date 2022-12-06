using NUnit.Framework;
using ProtoBuf;

namespace TNT.IntegrationTests.Serialization;

[ProtoContract]
public class Company
{
    [ProtoMember(3)]
    public string Name;
    [ProtoMember(2)]
    public int Id;
    [ProtoMember(1)]
    public User[] Users;
    public void AssertIsSameTo(Company company)
    {
        Assert.AreEqual(company.Name, Name);
        Assert.AreEqual(Id, company.Id);
        Assert.AreEqual(Users.Length, company.Users.Length);
        for (int i = 0; i < Users.Length; i++)
        {
            Users[i].AssertIsSameTo(company.Users[i]);
        }
    }
}