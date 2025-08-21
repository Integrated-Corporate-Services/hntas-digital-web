# HNTAS.Api.Client.Api.UsersApi

All URIs are relative to *https://localhost:7117*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiUsersContributorRolesGet**](UsersApi.md#apiuserscontributorrolesget) | **GET** /api/Users/contributor-roles |  |
| [**ApiUsersGet**](UsersApi.md#apiusersget) | **GET** /api/Users |  |
| [**ApiUsersIdDelete**](UsersApi.md#apiusersiddelete) | **DELETE** /api/Users/{id} |  |
| [**ApiUsersIdHeatnetworkHeatNetworkIdPatch**](UsersApi.md#apiusersidheatnetworkheatnetworkidpatch) | **PATCH** /api/Users/{id}/heatnetwork/{heatNetworkId} |  |
| [**ApiUsersIdInvitationPatch**](UsersApi.md#apiusersidinvitationpatch) | **PATCH** /api/Users/{id}/Invitation |  |
| [**ApiUsersIdOrgDetailsPatch**](UsersApi.md#apiusersidorgdetailspatch) | **PATCH** /api/Users/{id}/org-details |  |
| [**ApiUsersInitialEntryPost**](UsersApi.md#apiusersinitialentrypost) | **POST** /api/Users/initial-entry |  |
| [**ApiUsersOrganisationExistsGet**](UsersApi.md#apiusersorganisationexistsget) | **GET** /api/Users/organisation/exists |  |
| [**GetUserById**](UsersApi.md#getuserbyid) | **GET** /api/Users/{id} |  |
| [**GetUserByOneLoginId**](UsersApi.md#getuserbyoneloginid) | **GET** /api/Users/onelogin/{oneLoginId} |  |

<a id="apiuserscontributorrolesget"></a>
# **ApiUsersContributorRolesGet**
> List&lt;EnumItemResponse&gt; ApiUsersContributorRolesGet ()




### Parameters
This endpoint does not need any parameter.
### Return type

[**List&lt;EnumItemResponse&gt;**](EnumItemResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiusersget"></a>
# **ApiUsersGet**
> List&lt;UserResponse&gt; ApiUsersGet ()




### Parameters
This endpoint does not need any parameter.
### Return type

[**List&lt;UserResponse&gt;**](UserResponse.md)

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

<a id="apiusersidheatnetworkheatnetworkidpatch"></a>
# **ApiUsersIdHeatnetworkHeatNetworkIdPatch**
> void ApiUsersIdHeatnetworkHeatNetworkIdPatch (string id, string heatNetworkId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **string** |  |  |
| **heatNetworkId** | **string** |  |  |

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

<a id="apiusersidinvitationpatch"></a>
# **ApiUsersIdInvitationPatch**
> void ApiUsersIdInvitationPatch (string id, UpdateInvitationRequest updateInvitationRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **string** |  |  |
| **updateInvitationRequest** | [**UpdateInvitationRequest**](UpdateInvitationRequest.md) |  |  |

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
> string ApiUsersInitialEntryPost (InitialUserRegistrationRequest initialUserRegistrationRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **initialUserRegistrationRequest** | [**InitialUserRegistrationRequest**](InitialUserRegistrationRequest.md) |  |  |

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
| **400** | Bad Request |  -  |
| **409** | Conflict |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiusersorganisationexistsget"></a>
# **ApiUsersOrganisationExistsGet**
> bool ApiUsersOrganisationExistsGet (string companiesHouseNumber = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **companiesHouseNumber** | **string** |  | [optional]  |

### Return type

**bool**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getuserbyid"></a>
# **GetUserById**
> UserResponse GetUserById (string id)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **string** |  |  |

### Return type

[**UserResponse**](UserResponse.md)

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

<a id="getuserbyoneloginid"></a>
# **GetUserByOneLoginId**
> UserResponse GetUserByOneLoginId (string oneLoginId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **oneLoginId** | **string** |  |  |

### Return type

[**UserResponse**](UserResponse.md)

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

