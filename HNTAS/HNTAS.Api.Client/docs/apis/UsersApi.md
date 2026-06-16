# HNTAS.Api.Client.Api.UsersApi

All URIs are relative to *https://localhost:7117*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiUsersContributorRolesGet**](UsersApi.md#apiuserscontributorrolesget) | **GET** /api/Users/contributor-roles |  |
| [**ApiUsersGet**](UsersApi.md#apiusersget) | **GET** /api/Users |  |
| [**ApiUsersHeatNetworkHnIdRolesGet**](UsersApi.md#apiusersheatnetworkhnidrolesget) | **GET** /api/Users/heat-network/{hnId}/roles |  |
| [**ApiUsersIdDelete**](UsersApi.md#apiusersiddelete) | **DELETE** /api/Users/{id} |  |
| [**ApiUsersIdOrgDetailsPatch**](UsersApi.md#apiusersidorgdetailspatch) | **PATCH** /api/Users/{id}/org-details |  |
| [**ApiUsersIdUserDetailsPatch**](UsersApi.md#apiusersiduserdetailspatch) | **PATCH** /api/Users/{id}/user-details |  |
| [**ApiUsersInitialEntryPost**](UsersApi.md#apiusersinitialentrypost) | **POST** /api/Users/initial-entry |  |
| [**ApiUsersIsActiveUserEmailIdGet**](UsersApi.md#apiusersisactiveuseremailidget) | **GET** /api/Users/is-active-user/{emailId} |  |
| [**ApiUsersIsRpUserEmailIdGet**](UsersApi.md#apiusersisrpuseremailidget) | **GET** /api/Users/is-rp-user/{emailId} |  |
| [**ApiUsersManagedUsersGet**](UsersApi.md#apiusersmanagedusersget) | **GET** /api/Users/managed-users |  |
| [**ApiUsersNetworkManagersGet**](UsersApi.md#apiusersnetworkmanagersget) | **GET** /api/Users/network-managers |  |
| [**ApiUsersOrganisationExistsGet**](UsersApi.md#apiusersorganisationexistsget) | **GET** /api/Users/organisation/exists |  |
| [**ApiUsersOrganisationOrganisationIdGet**](UsersApi.md#apiusersorganisationorganisationidget) | **GET** /api/Users/organisation/{organisationId} |  |
| [**ApiUsersRegisterOrgAndLinkUserIdPost**](UsersApi.md#apiusersregisterorgandlinkuseridpost) | **POST** /api/Users/register-org-and-link/{userId} |  |
| [**ApiUsersRegisteredUsersGet**](UsersApi.md#apiusersregisteredusersget) | **GET** /api/Users/registered-users |  |
| [**ApiUsersUpdateOrgidPatch**](UsersApi.md#apiusersupdateorgidpatch) | **PATCH** /api/Users/update-orgid |  |
| [**ApiUsersUserDetailsByIdGet**](UsersApi.md#apiusersuserdetailsbyidget) | **GET** /api/Users/user-details-by-id |  |
| [**ApiUsersUserRolesGet**](UsersApi.md#apiusersuserrolesget) | **GET** /api/Users/user-roles |  |
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

<a id="apiusersheatnetworkhnidrolesget"></a>
# **ApiUsersHeatNetworkHnIdRolesGet**
> List&lt;UserRoleDetailResponse&gt; ApiUsersHeatNetworkHnIdRolesGet (string hnId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **hnId** | **string** |  |  |

### Return type

[**List&lt;UserRoleDetailResponse&gt;**](UserRoleDetailResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **404** | Not Found |  -  |

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
> User ApiUsersIdOrgDetailsPatch (string id, UpdateUserOrganisationRequest updateUserOrganisationRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **string** |  |  |
| **updateUserOrganisationRequest** | [**UpdateUserOrganisationRequest**](UpdateUserOrganisationRequest.md) |  |  |

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

<a id="apiusersiduserdetailspatch"></a>
# **ApiUsersIdUserDetailsPatch**
> void ApiUsersIdUserDetailsPatch (string id, UpdateUserDetailsRequest updateUserDetailsRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **string** |  |  |
| **updateUserDetailsRequest** | [**UpdateUserDetailsRequest**](UpdateUserDetailsRequest.md) |  |  |

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

<a id="apiusersisactiveuseremailidget"></a>
# **ApiUsersIsActiveUserEmailIdGet**
> bool ApiUsersIsActiveUserEmailIdGet (string emailId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **emailId** | **string** |  |  |

### Return type

**bool**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiusersisrpuseremailidget"></a>
# **ApiUsersIsRpUserEmailIdGet**
> bool ApiUsersIsRpUserEmailIdGet (string emailId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **emailId** | **string** |  |  |

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
| **404** | Not Found |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiusersmanagedusersget"></a>
# **ApiUsersManagedUsersGet**
> List&lt;ManagedUserResponse&gt; ApiUsersManagedUsersGet (string userId = null, bool networkManagersOnly = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **userId** | **string** |  | [optional]  |
| **networkManagersOnly** | **bool** |  | [optional] [default to false] |

### Return type

[**List&lt;ManagedUserResponse&gt;**](ManagedUserResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiusersnetworkmanagersget"></a>
# **ApiUsersNetworkManagersGet**
> List&lt;InvitedUserResponse&gt; ApiUsersNetworkManagersGet (string userId = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **userId** | **string** |  | [optional]  |

### Return type

[**List&lt;InvitedUserResponse&gt;**](InvitedUserResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **404** | Not Found |  -  |

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

<a id="apiusersorganisationorganisationidget"></a>
# **ApiUsersOrganisationOrganisationIdGet**
> List&lt;UserResponse&gt; ApiUsersOrganisationOrganisationIdGet (string organisationId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **organisationId** | **string** |  |  |

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
| **404** | Not Found |  -  |
| **400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiusersregisterorgandlinkuseridpost"></a>
# **ApiUsersRegisterOrgAndLinkUserIdPost**
> Organisation ApiUsersRegisterOrgAndLinkUserIdPost (string userId, OrganisationRequest organisationRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **userId** | **string** |  |  |
| **organisationRequest** | [**OrganisationRequest**](OrganisationRequest.md) |  |  |

### Return type

[**Organisation**](Organisation.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | Created |  -  |
| **400** | Bad Request |  -  |
| **404** | Not Found |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiusersregisteredusersget"></a>
# **ApiUsersRegisteredUsersGet**
> List&lt;UserResponse&gt; ApiUsersRegisteredUsersGet (string userId = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **userId** | **string** |  | [optional]  |

### Return type

[**List&lt;UserResponse&gt;**](UserResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiusersupdateorgidpatch"></a>
# **ApiUsersUpdateOrgidPatch**
> void ApiUsersUpdateOrgidPatch (UpdateUserOrgIdRequest updateUserOrgIdRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **updateUserOrgIdRequest** | [**UpdateUserOrgIdRequest**](UpdateUserOrgIdRequest.md) |  |  |

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **204** | No Content |  -  |
| **400** | Bad Request |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiusersuserdetailsbyidget"></a>
# **ApiUsersUserDetailsByIdGet**
> UserDetailsResponse ApiUsersUserDetailsByIdGet (string id = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **id** | **string** |  | [optional]  |

### Return type

[**UserDetailsResponse**](UserDetailsResponse.md)

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

<a id="apiusersuserrolesget"></a>
# **ApiUsersUserRolesGet**
> List&lt;EnumItemResponse&gt; ApiUsersUserRolesGet ()




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

