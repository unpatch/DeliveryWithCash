using HarmonyLib;
using Il2CppScheduleOne.Delivery;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.UI.Phone.Delivery;
using UnityEngine;
using static Il2CppScheduleOne.UI.Shop.ShopInterface;

namespace DeliveryWithCash
{
    [HarmonyPatch(typeof(DeliveryShop))]
    public static class DeliveryShopPatch
    {
        [HarmonyPatch(nameof(DeliveryShop.OrderPressed))]
        [HarmonyPrefix]
        public static bool OrderPressed(ref DeliveryShop __instance)
        {
            string reason;
            if (!__instance.CanOrder(out reason))
            {
                Debug.LogWarning("Cannot order: " + reason);
                return false;
            }

            float orderTotal = __instance.GetOrderTotal();
            List<StringIntPair> list = new List<StringIntPair>();
            foreach (ListingEntry listingEntry in __instance.listingEntries)
            {
                if (listingEntry.SelectedQuantity > 0)
                {
                    list.Add(new StringIntPair(listingEntry.MatchingListing.Item.ID, listingEntry.SelectedQuantity));
                }
            }
            int orderItemCount = __instance.GetOrderItemCount();
            int timeUntilArrival = Mathf.RoundToInt(Mathf.Lerp(60f, 360f, Mathf.Clamp01((float)orderItemCount / 160f)));
            DeliveryInstance delivery = new DeliveryInstance(Il2Cpp.GUIDManager.GenerateUniqueGUID().ToString(),
                                                             __instance.MatchingShopInterfaceName,
                                                             __instance.destinationProperty.PropertyCode,
                                                             __instance.loadingDockIndex - 1,
                                                             list.ToArray(),
                                                             EDeliveryStatus.InTransit,
                                                             timeUntilArrival);

            NetworkSingleton<DeliveryManager>.Instance.SendDelivery(delivery);

            if (__instance.MatchingShop.PaymentType == EPaymentType.Cash)
                NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(-orderTotal, true, false);
            else
                NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Delivery from " + __instance.MatchingShop.ShopName, -orderTotal, 1f, string.Empty);

            PlayerSingleton<DeliveryApp>.Instance.PlayOrderSubmittedAnim();
            __instance.ResetCart();

            return false;
        }

        [HarmonyPatch(nameof(DeliveryShop.CanOrder))]
        [HarmonyPrefix]
        public static bool CanOrder(ref DeliveryShop __instance, ref string reason, ref bool __result)
        {
            reason = string.Empty;
            if (__instance.HasActiveDelivery())
            {
                reason = "Delivery already in progress";
                __result = false;
                return false;
            }
            float cartCost = __instance.GetCartCost();

            if (__instance.MatchingShop.PaymentType == EPaymentType.Cash &&
                __instance.GetOrderTotal() > NetworkSingleton<MoneyManager>.Instance.cashBalance)
            {
                reason = "Insufficient cash";
                __result = false;
                return false;
            }
            else if (__instance.GetOrderTotal() > NetworkSingleton<MoneyManager>.Instance.sync___get_value_onlineBalance())
            {
                reason = "Insufficient online balance";
                __result = false;
                return false;
            }

            if (__instance.destinationProperty == null)
            {
                reason = "Select a destination";
                __result = false;
                return false;
            }
            if (__instance.destinationProperty.LoadingDockCount == 0)
            {
                reason = "Selected destination has no loading docks";
                __result = false;
                return false;
            }
            if (__instance.loadingDockIndex == 0)
            {
                reason = "Select a loading dock";
                __result = false;
                return false;
            }
            if (!__instance.WillCartFitInVehicle())
            {
                reason = "Order is too large for delivery vehicle";
                __result = false;
                return false;
            }
            __result = cartCost > 0f;
            return false;
        }
    }

}