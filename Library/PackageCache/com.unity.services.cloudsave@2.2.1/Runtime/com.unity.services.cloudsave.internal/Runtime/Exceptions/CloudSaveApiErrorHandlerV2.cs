using System;
using System.Collections.Generic;
using Unity.Services.CloudSave.Internal.Http;
using Unity.Services.CloudSave.Internal.Models;
using Unity.Services.Core;

namespace Unity.Services.CloudSave.Internal
{
    internal interface ICloudSaveApiErrorHandlerV2
    {
        bool IsRateLimited { get; }
        CloudSaveException HandleBasicResponseException(HttpException<BasicErrorResponse> response);
        CloudSaveException HandleGCSResponseException(HttpException<GCSErrorResponse> response);
        CloudSaveConflictException HandleDeleteConflictResponseException(HttpException<DeleteConflictErrorResponse> response);
        CloudSaveConflictException HandleBatchConflictResponseException(HttpException<BatchConflictErrorResponse> response);
        CloudSaveValidationException HandleValidationResponseException(HttpException<ValidationErrorResponse> response);
        CloudSaveValidationException HandleBatchValidationResponseException(HttpException<BatchValidationErrorResponse> response);
        CloudSaveException HandleDeserializationException(ResponseDeserializationException exception);
        CloudSaveException HandleHttpException(HttpException exception);
        CloudSaveException HandleException(Exception exception);
        CloudSaveRateLimitedException CreateRateLimitException();
    }

    internal class CloudSaveApiErrorHandlerV2 : ICloudSaveApiErrorHandlerV2
    {
        readonly IRateLimiter _rateLimiter;
        CloudSaveRateLimitedException _exception;

        public CloudSaveApiErrorHandlerV2(IRateLimiter rateLimiter)
        {
            _rateLimiter = rateLimiter;
        }

        public bool IsRateLimited => _rateLimiter.RateLimited;

        public CloudSaveRateLimitedException CreateRateLimitException()
        {
            if (_exception == null)
            {
                BasicErrorResponse error = new BasicErrorResponse("TooManyRequests", status: 429);
                HttpClientResponse response = new HttpClientResponse(new Dictionary<string, string>(), 429,
                    true, false, new byte[0], String.Empty);

                _exception = new CloudSaveRateLimitedException(GetReason(429), 429, GetGenericMessage(429),
                    _rateLimiter.RetryAfter, new HttpException<BasicErrorResponse>(response, error));
            }

            _exception.RetryAfter = _rateLimiter.RetryAfter;
            return _exception;
        }

        public CloudSaveException HandleHttpException(HttpException exception)
        {
            if (exception.Response.IsNetworkError)
            {
                string requestFailedMessage = "The request to the Cloud Save service failed - make sure you're connected to an internet connection and try again.";
                return new CloudSaveException(CloudSaveExceptionReason.NoInternetConnection, CommonErrorCodes.TransportError, requestFailedMessage, exception);
            }

            string message = exception.Response.ErrorMessage ?? GetGenericMessage(exception.Response.StatusCode);
            return new CloudSaveException(GetReason(exception.Response.StatusCode), CommonErrorCodes.Unknown, message, exception);
        }

        public CloudSaveException HandleException(Exception exception)
        {
            string message = "An unknown error occurred in the Cloud Save SDK.";

            return new CloudSaveException(CloudSaveExceptionReason.Unknown, CommonErrorCodes.Unknown, message, exception);
        }

        public CloudSaveException HandleDeserializationException(ResponseDeserializationException exception)
        {
            string message = exception.response.ErrorMessage ?? GetGenericMessage(exception.response.StatusCode);

            return new CloudSaveException(GetReason(exception.response.StatusCode), CommonErrorCodes.Unknown, message, exception);
        }

        public CloudSaveException HandleBasicResponseException(HttpException<BasicErrorResponse> response)
        {
            var message = String.IsNullOrEmpty(response.ActualError.Detail)
                ? GetGenericMessage(response.Response.StatusCode) : response.ActualError.Detail;

            if (_rateLimiter.IsRateLimitException(response))
            {
                _rateLimiter.ProcessRateLimit(response);
                _exception = new CloudSaveRateLimitedException(GetReason(response.Response.StatusCode),
                    response.ActualError.Code,
                    message, _rateLimiter.RetryAfter, response);

                return _exception;
            }

            return new CloudSaveException(GetReason(response.Response.StatusCode), response.ActualError.Code, message,
                response);
        }

        public CloudSaveException HandleGCSResponseException(HttpException<GCSErrorResponse> response)
        {
            var message = GetGenericMessage(response.Response.StatusCode);

            return new CloudSaveException(GetReason(response.Response.StatusCode), GetGCSCode(response.Response.StatusCode), message,
                response);
        }

        public CloudSaveConflictException HandleDeleteConflictResponseException(HttpException<DeleteConflictErrorResponse> response)
        {
            var message = String.IsNullOrEmpty(response.ActualError.Detail)
                ? GetGenericMessage(response.Response.StatusCode) : response.ActualError.Detail;
            var data = response.ActualError.Data;
            return new CloudSaveConflictException(GetReason(response.Response.StatusCode), response.ActualError.Code, message,
                new List<CloudSaveConflictErrorDetail>() { new CloudSaveConflictErrorDetail(data.Key, data.AttemptedWriteLock, data.ExistingWriteLock)},
                response);
        }

        public CloudSaveConflictException HandleBatchConflictResponseException(HttpException<BatchConflictErrorResponse> response)
        {
            var message = String.IsNullOrEmpty(response.ActualError.Detail)
                ? GetGenericMessage(response.Response.StatusCode) : response.ActualError.Detail;

            var details = new List<CloudSaveConflictErrorDetail>();
            foreach (var error in response.ActualError.Data)
            {
                details.Add(new CloudSaveConflictErrorDetail(error.Attempted.Key, error.Attempted.WriteLock, error.Existing.WriteLock));
            }

            return new CloudSaveConflictException(GetReason(response.Response.StatusCode), response.ActualError.Code, message, details,
                response);
        }

        public CloudSaveValidationException HandleValidationResponseException(HttpException<ValidationErrorResponse> response)
        {
            var message = "There was a validation error. Check 'Details' for more information.";

            CloudSaveValidationException exception = new CloudSaveValidationException(GetReason(response.Response.StatusCode),
                response.ActualError.Code, message, response);

            foreach (var error in response.ActualError.Errors)
            {
                exception.Details.Add(new CloudSaveValidationErrorDetail(error));
            }

            return exception;
        }

        public CloudSaveValidationException HandleBatchValidationResponseException(HttpException<BatchValidationErrorResponse> response)
        {
            var message = "There was a validation error. Check 'Details' for more information.";

            CloudSaveValidationException exception = new CloudSaveValidationException(GetReason(response.Response.StatusCode),
                response.ActualError.Code, message, response);

            foreach (var error in response.ActualError.Errors)
            {
                exception.Details.Add(new CloudSaveValidationErrorDetail(error));
            }

            return exception;
        }

        CloudSaveExceptionReason GetReason(long statusCode)
        {
            switch (statusCode)
            {
                case 400:
                    return CloudSaveExceptionReason.InvalidArgument;
                case 401:
                    return CloudSaveExceptionReason.Unauthorized;
                case 403:
                    return CloudSaveExceptionReason.KeyLimitExceeded;
                case 404:
                    return CloudSaveExceptionReason.NotFound;
                case 409:
                    return CloudSaveExceptionReason.Conflict;
                // GCS returns a 412 when a writeLock does not match, but we treat it as a conflict
                case 412:
                    return CloudSaveExceptionReason.Conflict;
                case 429:
                    return CloudSaveExceptionReason.TooManyRequests;
                case 500:
                case 503:
                    return CloudSaveExceptionReason.ServiceUnavailable;
                default:
                    return CloudSaveExceptionReason.Unknown;
            }
        }

        string GetGenericMessage(long statusCode)
        {
            switch (statusCode)
            {
                case 400:
                    return "Some of the arguments passed to the Cloud Save request were invalid. Please check the requirements and try again.";
                case 401:
                    return "Permission denied when making a request to the Cloud Save service. Ensure you are signed in through the Authentication SDK and try again.";
                case 403:
                    return "Key-value pair limit per user exceeded.";
                case 404:
                    return "The requested action could not be completed as the specified resource is not found - please make sure it exists, then try again.";
                case 409:
                    return "WriteLock in one or more data items within request does not match stored WriteLock.";
                case 412:
                    return "WriteLock in file upload request does not match stored WriteLock.";
                case 429:
                    return "Too many requests have been sent, so this device has been rate limited. Please try again later.";
                case 500:
                case 503:
                    return "Cloud Save service is currently unavailable. Please try again later.";
                default:
                    return "An unknown error occurred in the Cloud Save SDK.";
            }
        }

        int GetGCSCode(long statusCode)
        {
            return statusCode switch
            {
                404 => 7007,
                412 => 7012,
                _ => 1000
            };
        }
    }
}
