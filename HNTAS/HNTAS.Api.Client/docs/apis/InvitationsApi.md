# HNTAS.Api.Client.Api.InvitationsApi

All URIs are relative to *https://localhost:7117*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiInvitationsIdAddUserInvitationPost**](InvitationsApi.md#apiinvitationsidadduserinvitationpost) | **POST** /api/Invitations/{id}/add-user-invitation |  |
| [**ApiInvitationsIdGet**](InvitationsApi.md#apiinvitationsidget) | **GET** /api/Invitations/{id} |  |
| [**ApiInvitationsInvitationIdRejectPost**](InvitationsApi.md#apiinvitationsinvitationidrejectpost) | **POST** /api/Invitations/{invitationId}/Reject |  |
| [**ApiInvitationsInvitationIdSendEmailPost**](InvitationsApi.md#apiinvitationsinvitationidsendemailpost) | **POST** /api/Invitations/{invitationId}/send-email |  |

<a id="apiinvitationsidadduserinvitationpost"></a>
# **ApiInvitationsIdAddUserInvitationPost**
> string ApiInvitationsIdAddUserInvitationPost (string id, AddInvitationRequest addInvitationRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **string** |  |  |
| **addInvitationRequest** | [**AddInvitationRequest**](AddInvitationRequest.md) |  |  |

### Return type

**string**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | Created |  -  |
| **404** | Not Found |  -  |
| **400** | Bad Request |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiinvitationsidget"></a>
# **ApiInvitationsIdGet**
> InvitedUserResponse ApiInvitationsIdGet (string id)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **string** |  |  |

### Return type

[**InvitedUserResponse**](InvitedUserResponse.md)

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
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiinvitationsinvitationidrejectpost"></a>
# **ApiInvitationsInvitationIdRejectPost**
> void ApiInvitationsInvitationIdRejectPost (string invitationId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **invitationId** | **string** |  |  |

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **204** | No Content |  -  |
| **404** | Not Found |  -  |
| **400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiinvitationsinvitationidsendemailpost"></a>
# **ApiInvitationsInvitationIdSendEmailPost**
> void ApiInvitationsInvitationIdSendEmailPost (string invitationId, SendInvitationEmailRequest sendInvitationEmailRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **invitationId** | **string** |  |  |
| **sendInvitationEmailRequest** | [**SendInvitationEmailRequest**](SendInvitationEmailRequest.md) |  |  |

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **204** | No Content |  -  |
| **404** | Not Found |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

