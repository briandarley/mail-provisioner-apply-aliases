namespace mail_provisioner_apply_aliases.Pocos;
public class AccessToken
{

    public string access_token { get; set; }
    public int expires_in { get; set; }
    public string token_type { get; set; }
    public string scope { get; set; }
    public DateTime EmpireDateTime { get; set; }


    public override string ToString()
    {
        return $"{token_type} {access_token}";
    }
}