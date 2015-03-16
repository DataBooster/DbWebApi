/*!
* DbWebApi Client JavaScript Library v1.0.3-alpha
* https://github.com/databooster/dbwebapi
* Date: 2015-03-15
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
			var requestUri;

			if (inputJson == null || inputJson === "")
				requestUri = url;
			else {
				var tie = (url.lastIndexOf("?") === -1) ? "?" : "&";
				requestUri = url + tie + "JsonInput=" + encodeURIComponent(toJsonString(inputJson));
			}

			return jQuery.ajax({
				type: "GET",
				url: requestUri,
				processData: false,
				xhrFields: { withCredentials: true },
				success: successCallback,
				error: errorCallback
			});
		},

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
