/*!
* DbWebApi Client JavaScript Library v1.0.8-alpha
* https://github.com/databooster/dbwebapi
* Date: 2015-03-19
*/
(function (window, undefined) {
	jQuery.support.cors = true;

	function toJsonString(inputJson) {
		if (jQuery.isPlainObject(inputJson))
			return JSON.stringify(inputJson);
		else if (typeof inputJson === "string")
			return inputJson;
		else
			return "{}";
	}

	function appendJsonInputToUrl(url, inputObject, paramName) {
		if (!inputObject || inputObject === "")
			return url;
		if (!paramName)
			paramName = "JsonInput";
		var tie = (url.lastIndexOf("?") === -1) ? "?" : "&";
		return url + tie + paramName + "=" + encodeURIComponent((typeof inputObject === "string") ? inputObject : toJsonString(inputObject));
	}

	jQuery.extend({
		postDb: function (url, inputJson, successCallback, errorCallback) {
			return jQuery.ajax({
				type: "POST",
				url: url,
				contentType: "application/json;charset=utf-8",
				data: toJsonString(inputJson),
				processData: false,
				xhrFields: { withCredentials: true },
				success: successCallback,
				error: errorCallback
			});
		},

		getDb: function (url, inputJson, successCallback, errorCallback) {
			return jQuery.ajax({
				type: "GET",
				url: appendJsonInputToUrl(url, inputJson),
				processData: false,
				xhrFields: { withCredentials: true },
				success: successCallback,
				error: errorCallback
			});
		},

		jsonpDb: function (url, inputJson, successCallback, errorCallback) {
			var jpUrl = appendJsonInputToUrl(url, inputJson);
			return jQuery.ajax({
				type: "GET",
				url: jpUrl,
				dataType: "jsonp",
				processData: false,
				xhrFields: { withCredentials: true },
				success: successCallback,
				error: errorCallback
			});
		},

		appendJsonToUrl: appendJsonInputToUrl,

		utcDate: function (year, month/* 1-12 */, day, hour, minute, second, millisecond) {
			var date = new Date();
			if (!millisecond) {
				millisecond = 0;
				if (!second) {
					second = 0;
					if (!minute) {
						minute = 0;
						if (!hour) {
							hour = 0;
							if (!day) {
								day = 1;
								if (!month) {
									month = 1; // 1-12
									if (!year) {
										year = date.getFullYear();
										month = date.getMonth() + 1; // 0-11 --> 1-12
										day = date.getDate();
									} else if ($.type(year) === "date") {
										millisecond = year.getMilliseconds();
										second = year.getSeconds();
										minute = year.getMinutes();
										hour = year.getHours();
										day = year.getDate();
										month = year.getMonth() + 1; // 0-11 --> 1-12
										year = year.getFullYear();
									}
								}
							}
						}
					}
				}
			}
			date.setUTCFullYear(year, month - 1 /* 1-12 --> 0-11 */, day);
			date.setUTCHours(hour, minute, second, millisecond);
			return date;
		}
	});
})(window);
