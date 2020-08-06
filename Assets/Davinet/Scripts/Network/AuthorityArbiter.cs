using System.Collections.Generic;

// TODO: Move to interface.
public class AuthorityArbiter
{
    private HashSet<int> localAuthorities;

    public AuthorityArbiter()
    {
        localAuthorities = new HashSet<int>();
    }

    public void AddLocalAuthority(int peerId)
    {
        localAuthorities.Add(peerId);
    }

    public bool CanWrite(int authority)
    {
        return localAuthorities.Contains(authority);
    }
}
