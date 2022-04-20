/*
 *
 * Copyright (c) Microsoft Corporation.
 * All rights reserved.
 *
 * This code is licensed under the MIT License.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files(the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions :
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *
 */
/**
 * @brief Defines custom telemetry or audit configurations
 * 
 * @file diagnostic_configuration.h
 */

#ifndef API_MIP_DIAGNOSTIC_CONFIGURATION_H_
#define API_MIP_DIAGNOSTIC_CONFIGURATION_H_

#include <map>
#include <memory>

#include "mip/audit_delegate.h"
#include "mip/http_delegate.h"
#include "mip/mip_namespace.h"
#include "mip/task_dispatcher_delegate.h"
#include "mip/telemetry_delegate.h"


MIP_NAMESPACE_BEGIN

/**
 * @brief Custom diagnostic configurations (not commonly used)
 */

struct DiagnosticConfiguration {
  std::string hostNameOverride; /**< Host audit/telemetry instance name. If not set, MIP will act as its own host. */
  std::string libraryNameOverride; /**< Alternate audit/telemetry library (DLL) filename. */
  std::shared_ptr<HttpDelegate> httpDelegateOverride; /**< If set, HTTP handling will be managed by this instance*/
  std::shared_ptr<TaskDispatcherDelegate> taskDispatcherDelegateOverride; /**< If set, async task handling will be managed by this instance,
      taskDispatcherDelegateOverides should not be shared as they can hold audit/telemetry objects, and prevent their release until taskDispatcher is freed.*/
  bool isNetworkDetectionEnabled {true}; /**< If set, audit/telemetry component will ping network status on background thread*/
  bool isLocalCachingEnabled {true}; /**< If set, audit/telemetry component will use on-disk caching */
  bool isTraceLoggingEnabled {false}; /**< If set, audit/telemetry component will write warning/error logs to disk */
  bool isMinimalTelemetryEnabled{ false }; /**< If set, only necessary service data telemetry will be sent */
  bool isFastShutdownEnabled {false}; /**< If set, No events will be uploaded on shutdown, Audit events will be uploaded immediately upon logging */
  std::map<std::string, std::string> customSettings; /**< Custom audit/telemetry settings >**/
  std::map<std::string, std::vector<std::string>> maskedProperties; /**< Audit/Telemetry events/properties which should be masked */

  std::shared_ptr<AuditDelegate> auditPipelineDelegateOverride;  /**< Audit delegate override for writting audit events */
  std::shared_ptr<TelemetryDelegate> telemetryPipelineDelegateOverride;  /**< Telemetry delegate override for writting telemetry events */

  Cloud cloud {Cloud::Unknown};  /**< Cloud type for controlling telemetry and audit events for sovereign cloud scenario */
};

MIP_NAMESPACE_END
#endif  // API_MIP_DIAGNOSTIC_CONFIGURATION_H_