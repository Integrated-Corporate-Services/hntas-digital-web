# HNTAS.Api.Client.Api.ArmsDashboardApi

All URIs are relative to *https://localhost:7117*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiArmsDashboardGetKpiNetworkDetailsGet**](ArmsDashboardApi.md#apiarmsdashboardgetkpinetworkdetailsget) | **GET** /api/ArmsDashboard/get-kpi-network-details |  |
| [**ApiArmsDashboardGetKpiNetworksByRpUserGet**](ArmsDashboardApi.md#apiarmsdashboardgetkpinetworksbyrpuserget) | **GET** /api/ArmsDashboard/get-kpi-networks-by-rp-user |  |
| [**ApiArmsDashboardSubmissionIdHistoryGet**](ArmsDashboardApi.md#apiarmsdashboardsubmissionidhistoryget) | **GET** /api/ArmsDashboard/{submissionId}/history |  |

<a id="apiarmsdashboardgetkpinetworkdetailsget"></a>
# **ApiArmsDashboardGetKpiNetworkDetailsGet**
> HeatNetworkDetailsResponse ApiArmsDashboardGetKpiNetworkDetailsGet (string submissionId = null, string statusFilter = null, string typeFilter = null, int page = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **submissionId** | **string** |  | [optional]  |
| **statusFilter** | **string** |  | [optional]  |
| **typeFilter** | **string** |  | [optional]  |
| **page** | **int** |  | [optional] [default to 1] |

### Return type

[**HeatNetworkDetailsResponse**](HeatNetworkDetailsResponse.md)

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
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiarmsdashboardgetkpinetworksbyrpuserget"></a>
# **ApiArmsDashboardGetKpiNetworksByRpUserGet**
> HeatNetworkDashboardResponse ApiArmsDashboardGetKpiNetworksByRpUserGet (string userId = null, int month = null, int year = null, int pageNumber = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **userId** | **string** |  | [optional]  |
| **month** | **int** |  | [optional]  |
| **year** | **int** |  | [optional]  |
| **pageNumber** | **int** |  | [optional] [default to 1] |

### Return type

[**HeatNetworkDashboardResponse**](HeatNetworkDashboardResponse.md)

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
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiarmsdashboardsubmissionidhistoryget"></a>
# **ApiArmsDashboardSubmissionIdHistoryGet**
> List&lt;KpiHistoryResponse&gt; ApiArmsDashboardSubmissionIdHistoryGet (string submissionId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **submissionId** | **string** |  |  |

### Return type

[**List&lt;KpiHistoryResponse&gt;**](KpiHistoryResponse.md)

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

