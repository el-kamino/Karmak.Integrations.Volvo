using System.Linq;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.React.Utils;

namespace Karmak.Integrations.Volvo.React.Core.Mappers.RepairOrder.V5_14_4
{
    public static class PrimaryDriverMapper
    {
        public static PrimaryDriverType From(Contact driver, RegionSettings region)
        {
            if (DriverDataPrecludesInclusionForVolvo(driver))
            {
                return null;
            }

            return new PrimaryDriverType
            {
                DriverParty = new PartyABIEType
                {
                    Item = WithWorkPhoneAsCellPhoneFusionHack(ContactMapper.MapFrom(driver, region))
                }
            };
        }

        /// <summary>
        /// We need to handle incoming driver data that is all null or otherwise unfit for transmission to Volvo
        /// by omitting the driver from the data we send.
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        private static bool DriverDataPrecludesInclusionForVolvo(Contact driver)
        {
            if (driver == null)
                return true;
            if (string.IsNullOrWhiteSpace(driver.LicenseNumber))
                return true;
            if (string.IsNullOrWhiteSpace(driver.LastName))
                return true;
            if (driver.Addresses==null)
                return true;
            if (string.IsNullOrWhiteSpace(driver.Addresses.FirstOrDefault()?.Street1))
                return true;
            if (string.IsNullOrWhiteSpace(driver.Addresses.FirstOrDefault()?.City))
                return true;
            if (string.IsNullOrWhiteSpace(driver.Addresses.FirstOrDefault()?.Region))
                return true;
            return false;
        }

        private static PersonTypeStar WithWorkPhoneAsCellPhoneFusionHack(PersonTypeStar person)
        {
            CommunicationABIETypeStar cell = person.TelephoneCommunication.FirstOrDefault(phone => phone.ChannelCode.Value == PhoneType.CELL.ToChannelCode());
            if (cell != null)
                cell.ChannelCode.Value = PhoneType.WORK.ToChannelCode();

            CommunicationABIETypeStar work = person.TelephoneCommunication.FirstOrDefault(phone => phone.ChannelCode.Value == PhoneType.WORK.ToChannelCode());
            if (work != null)
                work.ChannelCode.Value = PhoneType.CELL.ToChannelCode();

            return person;
        }
    }

}