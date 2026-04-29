# HNTAS.Api.Client.Api.NotificationHistoryApi

All URIs are relative to *https://localhost:7117*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiNotificationHistoryNotificationHistoryGet**](NotificationHistoryApi.md#apinotificationhistorynotificationhistoryget) | **GET** /api/NotificationHistory/notification-history |  |
| [**ApiNotificationHistoryUnreadNotificationCountGet**](NotificationHistoryApi.md#apinotificationhistoryunreadnotificationcountget) | **GET** /api/NotificationHistory/unread-notification-count |  |

<a id="apinotificationhistorynotificationhistoryget"></a>
# **ApiNotificationHistoryNotificationHistoryGet**
> NotificationHistoryResponse ApiNotificationHistoryNotificationHistoryGet (NotificationHistoryRequest notificationHistoryRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **notificationHistoryRequest** | [**NotificationHistoryRequest**](NotificationHistoryRequest.md) |  |  |

### Return type

[**NotificationHistoryResponse**](NotificationHistoryResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apinotificationhistoryunreadnotificationcountget"></a>
# **ApiNotificationHistoryUnreadNotificationCountGet**
> int ApiNotificationHistoryUnreadNotificationCountGet (string userId = null, UserRole role = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **userId** | **string** |  | [optional]  |
| **role** | **UserRole** |  | [optional]  |

### Return type

**int**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

