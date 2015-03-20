using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace BetaSeriesW8.Service
{

    /// <summary>
    /// Base badge notification content interface.
    /// </summary>
    public interface IBadgeNotificationContent : INotificationContent
    {
#if !WINRT_NOT_PRESENT
        /// <summary>
        /// Creates a WinRT BadgeNotification object based on the content.
        /// </summary>
        /// <returns>A WinRT BadgeNotification object based on the content.</returns>
        BadgeNotification CreateNotification();
#endif
    }

    /// <summary>
    /// Notification content object to display a number on a tile's badge.
    /// </summary>
    public sealed class BadgeNumericNotificationContent : IBadgeNotificationContent
    {
        /// <summary>
        /// Default constructor to create a numeric badge content object.
        /// </summary>
        public BadgeNumericNotificationContent()
        {
        }

        /// <summary>
        /// Constructor to create a numeric badge content object with a number.
        /// </summary>
        /// <param name="number">
        /// The number that will appear on the badge.  If the number is 0, the badge
        /// will be removed.  The largest value that will appear on the badge is 99.
        /// Numbers greater than 99 are allowed, but will be displayed as "99+".
        /// </param>
        public BadgeNumericNotificationContent(uint number)
        {
            m_Number = number;
        }

        /// <summary>
        /// The number that will appear on the badge.  If the number is 0, the badge
        /// will be removed.  The largest value that will appear on the badge is 99.
        /// Numbers greater than 99 are allowed, but will be displayed as "99+".
        /// </summary>
        public uint Number
        {
            get { return m_Number; }
            set { m_Number = value; }
        }

        /// <summary>
        /// Retrieves the notification Xml content as a string.
        /// </summary>
        /// <returns>The notification Xml content as a string.</returns>
        public string GetContent()
        {
            return String.Format("<badge version='{0}' value='{1}'/>",1, m_Number);
        }

        /// <summary>
        /// Retrieves the notification Xml content as a string.
        /// </summary>
        /// <returns>The notification Xml content as a string.</returns>
        public override string ToString()
        {
            return GetContent();
        }

#if !WINRT_NOT_PRESENT
        /// <summary>
        /// Retrieves the notification Xml content as a WinRT Xml document.
        /// </summary>
        /// <returns>The notification Xml content as a WinRT Xml document.</returns>
        public XmlDocument GetXml()
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(GetContent());
            return xml;
        }

        /// <summary>
        /// Creates a WinRT BadgeNotification object based on the content.
        /// </summary>
        /// <returns>A WinRT BadgeNotification object based on the content.</returns>
        public BadgeNotification CreateNotification()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(GetContent());
            return new BadgeNotification(xmlDoc);
        }
#endif

        private uint m_Number = 0;
    }
}
