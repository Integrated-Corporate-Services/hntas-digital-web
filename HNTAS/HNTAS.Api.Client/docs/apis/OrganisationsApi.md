# HNTAS.Api.Client.Api.OrganisationsApi

All URIs are relative to *https://localhost:7117*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiOrganisationsOrgIdEditOrgDetailsPatch**](OrganisationsApi.md#apiorganisationsorgideditorgdetailspatch) | **PATCH** /api/Organisations/{orgId}/edit-org-details |  |

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

