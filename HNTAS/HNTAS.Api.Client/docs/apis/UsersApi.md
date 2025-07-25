# HNTAS.Api.Client.Api.UsersApi

All URIs are relative to *https://localhost:7117*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiUsersGet**](UsersApi.md#apiusersget) | **GET** /api/Users |  |
| [**ApiUsersIdDelete**](UsersApi.md#apiusersiddelete) | **DELETE** /api/Users/{id} |  |
| [**ApiUsersIdOrgDetailsPatch**](UsersApi.md#apiusersidorgdetailspatch) | **PATCH** /api/Users/{id}/org-details |  |
| [**ApiUsersInitialEntryPost**](UsersApi.md#apiusersinitialentrypost) | **POST** /api/Users/initial-entry |  |
| [**GetUserById**](UsersApi.md#getuserbyid) | **GET** /api/Users/{id} |  |

<a id="apiusersget"></a>
# **ApiUsersGet**
> List&lt;User&gt; ApiUsersGet ()




### Parameters
This endpoint does not need any parameter.
### Return type

[**List&lt;User&gt;**](User.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiusersiddelete"></a>
# **ApiUsersIdDelete**
> void ApiUsersIdDelete (string id)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **string** |  |  |

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
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiusersidorgdetailspatch"></a>
# **ApiUsersIdOrgDetailsPatch**
> User ApiUsersIdOrgDetailsPatch (string id, UpdateOrgDetailsAndRolesRequest updateOrgDetailsAndRolesRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **string** |  |  |
| **updateOrgDetailsAndRolesRequest** | [**UpdateOrgDetailsAndRolesRequest**](UpdateOrgDetailsAndRolesRequest.md) |  |  |

### Return type

[**User**](User.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **404** | Not Found |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiusersinitialentrypost"></a>
# **ApiUsersInitialEntryPost**
> User ApiUsersInitialEntryPost (InitialUserRegistrationRequest initialUserRegistrationRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **initialUserRegistrationRequest** | [**InitialUserRegistrationRequest**](InitialUserRegistrationRequest.md) |  |  |

### Return type

[**User**](User.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | Created |  -  |
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **409** | Conflict |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getuserbyid"></a>
# **GetUserById**
> User GetUserById (string id)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **string** |  |  |

### Return type

[**User**](User.md)

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

