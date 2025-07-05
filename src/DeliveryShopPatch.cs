using HarmonyLib;
using UnityEngine;
using System.Reflection;

#if IL2CPP
using Il2Cpp;
using Il2CppScheduleOne.Delivery;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.Property;
using Il2CppScheduleOne.UI.Phone.Delivery;
using static Il2CppScheduleOne.UI.Shop.ShopInterface;
#elif MONO
using ScheduleOne.Delivery;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.Property;
using ScheduleOne.UI.Phone.Delivery;
using static ScheduleOne.UI.Shop.ShopInterface;
#endif

namespace DeliveryWithCash;

[HarmonyPatch(typeof(DeliveryShop))]
public static class DeliveryShopPatch
{
    public static MethodInfo GetOrderTotal = typeof(DeliveryShop).GetMethod("GetOrderTotal", BindingFlags.NonPublic | BindingFlags.Instance);
    public static MethodInfo GetOrderItemCount = typeof(DeliveryShop).GetMethod("GetOrderItemCount", BindingFlags.NonPublic | BindingFlags.Instance);

    [HarmonyPatch(nameof(DeliveryShop.OrderPressed))]
    [HarmonyPrefix]
    public static void OrderPressed(ref DeliveryShop __instance,
                                    ref bool __runOriginal,
                                    ref List<ListingEntry> ___listingEntries,
                                    ref Property ___destinationProperty,
                                    ref int ___loadingDockIndex)
    {
        __runOriginal = false;
        string reason;
        if (!__instance.CanOrder(out reason))
        {
            Debug.LogWarning("Cannot order: " + reason);
            return;
        }

        float orderTotal = (float)GetOrderTotal.Invoke(__instance, null);
        List<StringIntPair> list = new List<StringIntPair>();
        foreach (ListingEntry listingEntry in ___listingEntries)
        {
            if (listingEntry.SelectedQuantity > 0)
            {
                list.Add(new StringIntPair(listingEntry.MatchingListing.Item.ID, listingEntry.SelectedQuantity));
            }
        }
        int orderItemCount = (int)GetOrderItemCount.Invoke(__instance, null);
        int timeUntilArrival = Mathf.RoundToInt(Mathf.Lerp(60f, 360f, Mathf.Clamp01((float)orderItemCount / 160f)));
        DeliveryInstance delivery = new DeliveryInstance(GUIDManager.GenerateUniqueGUID().ToString(),
                                                         __instance.MatchingShopInterfaceName,
                                                         ___destinationProperty.PropertyCode,
                                                         ___loadingDockIndex - 1,
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
    }

    [HarmonyPatch(nameof(DeliveryShop.CanOrder))]
    [HarmonyPrefix]
    public static void CanOrder(ref DeliveryShop __instance,
                                ref string reason,
                                ref bool __result,
                                ref bool __runOriginal)
    {
        reason = string.Empty;
        if (__instance.HasActiveDelivery())
        {
            reason = "Delivery already in progress";
            __result = false;
            __runOriginal = false;
            return;
        }

        if (__instance.MatchingShop.PaymentType == EPaymentType.Cash &&
            (float)GetOrderTotal.Invoke(__instance, null) > NetworkSingleton<MoneyManager>.Instance.cashBalance)
        {
            reason = "Insufficient cash";
            __result = false;
            __runOriginal = false;
            return;
        }
    }
}