# HNTAS.Api.Client.Api.OrganisationsApi

All URIs are relative to *https://localhost:7117*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiOrganisationsExistsByDetailsGet**](OrganisationsApi.md#apiorganisationsexistsbydetailsget) | **GET** /api/Organisations/exists-by-details |  |
| [**ApiOrganisationsOrgIdEditOrgDetailsPatch**](OrganisationsApi.md#apiorganisationsorgideditorgdetailspatch) | **PATCH** /api/Organisations/{orgId}/edit-org-details |  |
| [**ApiOrganisationsOrgIdGet**](OrganisationsApi.md#apiorganisationsorgidget) | **GET** /api/Organisations/{orgId} |  |
| [**ApiOrganisationsOrgIdUserUserIdHeatnetworkHeatNetworkIdPatch**](OrganisationsApi.md#apiorganisationsorgiduseruseridheatnetworkheatnetworkidpatch) | **PATCH** /api/Organisations/{orgId}/user/{userId}/heatnetwork/{heatNetworkId} |  |
| [**ApiOrganisationsSearchGet**](OrganisationsApi.md#apiorganisationssearchget) | **GET** /api/Organisations/search |  |

<a id="apiorganisationsexistsbydetailsget"></a>
# **ApiOrganisationsExistsByDetailsGet**
> bool ApiOrganisationsExistsByDetailsGet (string name = null, string postCode = null, string country = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **name** | **string** |  | [optional]  |
| **postCode** | **string** |  | [optional]  |
| **country** | **string** |  | [optional]  |

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

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiorganisationsorgideditorgdetailspatch"></a>
# **ApiOrganisationsOrgIdEditOrgDetailsPatch**
> User ApiOrganisationsOrgIdEditOrgDetailsPatch (string orgId, OrganisationRequest organisationRequest, string userId = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **orgId** | **string** |  |  |
| **organisationRequest** | [**OrganisationRequest**](OrganisationRequest.md) |  |  |
| **userId** | **string** |  | [optional]  |

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

<a id="apiorganisationsorgidget"></a>
# **ApiOrganisationsOrgIdGet**
> Organisation ApiOrganisationsOrgIdGet (string orgId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **orgId** | **string** |  |  |

### Return type

[**Organisation**](Organisation.md)

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

<a id="apiorganisationsorgiduseruseridheatnetworkheatnetworkidpatch"></a>
# **ApiOrganisationsOrgIdUserUserIdHeatnetworkHeatNetworkIdPatch**
> void ApiOrganisationsOrgIdUserUserIdHeatnetworkHeatNetworkIdPatch (string orgId, string userId, string heatNetworkId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **orgId** | **string** |  |  |
| **userId** | **string** |  |  |
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

<a id="apiorganisationssearchget"></a>
# **ApiOrganisationsSearchGet**
> Organisation ApiOrganisationsSearchGet (string term = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **term** | **string** |  | [optional]  |

### Return type

[**Organisation**](Organisation.md)

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

