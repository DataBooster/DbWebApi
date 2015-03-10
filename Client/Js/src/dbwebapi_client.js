/*!
* DbWebApi Client JavaScript Library v1.0.2-alpha
* https://github.com/databooster/dbwebapi
* Date: 2015-03-10
*/
(function (window, undefined) {
	jQuery.support.cors = true;

	jQuery.extend({
		postDb: function (url, inputJson, successCallback, errorCallback) {
			var jsonContent;

			if (jQuery.isPlainObject(inputJson))
				jsonContent = JSON.stringify(inputJson);
			else if (typeof inputJson === "string")
				jsonContent = inputJson;
			else
				jsonContent = "{}";

			return jQuery.ajax({
				type: "POST",
				url: url,
				contentType: "application/json;charset=utf-8",
				data: jsonContent,
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
