namespace SalonApi.Models;

/// <summary>Editable text for a public page. The Key is "home" or "contact".</summary>
public class ContentPage
{
    public string Key { get; set; } = "";     // primary key, set explicitly in the DbContext
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";    // paragraphs separated by a blank line
}
