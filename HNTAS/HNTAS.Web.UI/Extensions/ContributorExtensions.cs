using HNTAS.Web.UI.Models.Enums;
using HNTAS.Web.UI.Models.User;

namespace HNTAS.Web.UI.Extensions
{
    public static class ContributorExtensions
    {
        /// <summary>
        /// Gets the display contact number based on the preferred contact type.
        /// </summary>
        /// <param name="contact">The ContributorContactDetailsModel instance.</param>
        /// <returns>A formatted string of the contact number, or "Not provided" if not available.</returns>
        public static string GetDisplayContactNumber(this ContributorContactDetailsModel contact)
        {
            if (contact == null)
            {
                return "Not provided";
            }

            switch (contact.PreferredContactType)
            {
                case PreferredContactType.Landline:
                    if (string.IsNullOrWhiteSpace(contact.ContactNumberExtension))
                    {
                        return contact.LandlineNumber;
                    }
                    else
                    {
                        return $"{contact.LandlineNumber} ext {contact.ContactNumberExtension}";
                    }
                case PreferredContactType.Mobile:
                    return contact.MobileNumber;
                default:
                    return "Not provided";
            }
        }
    }
}
