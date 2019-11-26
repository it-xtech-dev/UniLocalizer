/** ********************************************** **
	@Author			Paweł Kołodziej
	@Website		www.it.xtech.pl
    @Copyright      xtech.pl, do not distribute, copy or use without prmission
	@Last Update	20180912
	@requires: 	
					https://datatables.net - jQuery DataTables library
*************************************************** **/

(function( factory ) {
	"use strict";

	if ( typeof define === 'function' && define.amd ) {
		// AMD
		define( ['jquery'], function ( $ ) {
			return factory( $, window, document );
		} );
	}
	else if ( typeof exports === 'object' ) {
		// CommonJS
		module.exports = function (root, $) {
			if ( ! root ) {
				// CommonJS environments without a window global must pass a
				// root. This will give an error otherwise
				root = window;
			}

			if ( ! $ ) {
				$ = typeof window !== 'undefined' ? // jQuery's factory checks for a global window
					require('jquery') :
					require('jquery')( root );
			}

			return factory( $, root, root.document );
		};
	}
	else {
		// Browser
		factory( jQuery, window, document );
	}
}
(function( $, window, document, undefined ) {
        "use strict";

        var _baseDataTable = $.fn.dataTable;

        // *** DATATABLES WRAPPER ***
        // Wraps original DataTables constructor. ASSUMING THIS WILL NOT BREAK ogirginal DataTables funcionallity
        // so that settings could be modified with additional dynamic columns
        $.fn.dataTable = function (settings) {
            console.log('datatable constructor override!');
            // will add extra (dynamic) columns with deynamic columns are defined
            _fnInitializeDynamicColumns(settings);

            // preserve original stateSaveParams callback and add new one for handling dynamic columns
            if (typeof settings.stateSaveParams === 'function') {
                var baseStateSaveParams = settings.stateSaveParams;
                settings.stateSaveParams = function (settings, data) {
                    _fnStateSaveHandler(settings.oInstance.api(), data);
                    baseStateSaveParams(settings, data);
                };
            } else {
                settings.stateSaveParams = function (settings, data) {
                    _fnStateSaveHandler(settings.oInstance.api(), data);
                };
            }

            // preserve original createdRow callback and add new one for row-lines and transferring css class to parent (conditional formatting)
            var baseCreatedRowHandler = settings.createdRow;
            settings.createdRow = _fnCreatedRowHandler(settings, baseCreatedRowHandler);

            // return original datatable instance
            return _baseDataTable.apply(this,[settings]);
        };
        // Remap already present "static" properties
        $.extend($.fn.dataTable, _baseDataTable);
        // Wraps _fnCallbackFire so that custom stateLoadParmase handled could be called every time options callback is called
        // See Datatables source code for detials.
        //$.fn.DataTable.ext.internal._fnCallbackFire = function(settings, callbackArr, eventName, args) {
        //    if (callbackArr === 'aoStateLoadParams' && eventName === 'stateLoadParams') {
        //        _fnStateSaveHandler(settings.oInstance.api(), args[1]);
        //    } else {
        //        return _baseFnStateSave(settings, callbackArr, eventName, args);
        //    }
        //}

        // *** INITIALIZATION ***
        // When any of the tables on the page initializes
        $(document).on('init.dt', function (e, settings, json) {
            if (e.namespace !== 'dt') {
                return;
            }
            var api = new $.fn.dataTable.Api(settings);
            var $table = $(api.table().node());

            // DYNAMIC COLUMNS: 
            // assign right names and visibility for dynamic columns;
            _fnMapDynamicColumns(api);

            // COLUMN WIDTH: 
            // Set initial table and column widths
            // Managing columns widths is based on col element
            // col element is deprecated in html5 but seems to be still supported by all major browsers
            // during initialization col elements are added for all table columns (both visible and hidded)
            var cols = '';
            api.columns().every(function () {
                var css = (this.visible() && this.isEnabled()) ? '' : ' class="hidden"';
                cols += '<col width="' + this.width() + 'px"' + css + '>';
            });
            $table.prepend('<colgroup>' + cols + '</colgroup>');

            // ROW LINES: set default numer of row lines form settings
            api.rowLines(api.settings()[0].oInit.rowLines);
            if (typeof onDone == 'function') {
                // fire done handler when table initialized
                onDone(api);
            }

            // FIXED HEADER: 
            // TODO: this is limited to SINGLE datatables on page - needs refactoring for multiple tables
            // extract table header from host table to external table container
            $('#fixedHeader').append('<table class="table table-striped table-bordered dataTable"></table>');
            $table.find('thead').appendTo('#fixedHeader table');
            $('#scrollContainer').on('scroll', function () {
                $('#fixedHeader table').css('left', -this.scrollLeft);
            });

            // Make table visible (hidding table during initialization may improve performace a bit)
            $table.removeClass('hidden');

            // TABLE INNER WIDTH:
            // Setting fixed table width to ensure cells will have exact size
            // Html table behavior assumes streching cell bypassing their explicit width when table container is wider than all cells.
            // In this case this is unexpected behavior and setting exact table width leaves to space to cells streching.
            api.setWidth();

            // FIXED HEADER:
            // calculate and set width at initialize
            _fnSetFixedHeaderWidth();
            // calculate and set body offeset depending header height
            _fnSetBodyHeaderOffset();

            // *** DATATABLE EVENT HANDLERS ***
            // recalculate table width on column visibility changes
            $table.on('column-visibility.dt', function (e, settings, column, state) {
                var col = $table.find('colgroup > col:eq(' + column + ')');
                if (state) {
                    $table.find('tbody td:nth-child(' + (column + 1) + ') > .cell[data-parent-css-class]').each(function () {
                        var target = $(this);
                        var cssClass = target.attr('data-parent-css-class');
                        target.parent().addClass(cssClass);
                    });
                    col.removeClass('hidden');
                } else {
                    col.addClass('hidden');
                }
                api.setWidth();
            });
            // ROW LINES: 
            // Bind datatable cell expander action
            $table.on('click', '.cell-expander', function () {
                var expander = $(this);
                expander.closest('tr').toggleClass('row-expanded');
                return false;
            });
            // SELECTION:
            // Toggle selection when select header is clicked
            $('#fixedHeader th.select-checkbox').click(function () {
                var api = viewData.dataTable.api;
                var selectedRows = api.rows({ selected: true });
                if (selectedRows.count() > 0) {
                    selectedRows.deselect();
                } else {
                    api.rows({ search: 'applied' }).select();
                }

            });
            // FIXEDHEADER: Recalculate fixed header when window size changes
            $(window).on('resize', function () {
                //dataTableSetHeight();
                _fnSetFixedHeaderWidth();
            });
            /* PROCESSING: dirty hack to overide display:block that is set by datatables.js */
            var $processing = $(settings.aanFeatures.r);
            $processing.addClass("hidden");
            $table.on('processing.dt', function (e, settings, processing) {
                if (!processing) {
                    $processing.addClass("hidden");
                    console.timeEnd('processing');
                } else {
                    $processing.removeClass("hidden");
                    console.time('processing');
                }
            })
        });

        /** Adds dynamic columns to data table settings
        * @param settings - data table initial settings
        */
        function _fnInitializeDynamicColumns(settings) {
            if (typeof settings.dynamicColumns === "object") {
                var dynColSettings = settings.dynamicColumns;


                // Below code will add dynamic column containers to datatable
                // Dynamic columns can change its content and title on runtime
                // Number of dynamic columns is limited to finite number due to performance issues (this value can be changed in appsettings.json)
                // Not all columns are required in each scenario, not used columns will stay hidden based on isEnabled evaluator set in options
                // CAUTION: dynamic columns MUST BE added after static columns, and cannot be mixed with static columns.
                // NOT IMPLEMENTED: In further process it is possible to reorded dynamic columns position so that they can be placed anywhere in datatable
                var dynamicColumnCount = dynColSettings.columnCount;
                for (var index = 0; index < dynamicColumnCount; index++) {
                    var dynamicRender = new (function (colIndex) {
                        return function (data, type, row, meta) {
                            return dynColSettings.render(colIndex, type, row, meta);
                        };
                    })(index); // function is autoexecuted with proper inceremented column index.

                    var colWidth = "100px";
                    var widthSetting = dynColSettings.width;
                    if (typeof widthSetting === "string") {
                        colWidth = widthSetting;
                    } else if (typeof widthSetting === "function") {
                        colWidth = widthSetting(index)
                    }

                    settings.columns.push({
                        data: undefined,
                        title: 'col' + index,       // title and name will be changed at runtime
                        name: '_name' + index,       // CAUTION: _ prefix marks columns that are not mapped (not in use) - initiall note of dynamic columns is used (the wil get right names as soon as initialization proceeds)
                        orderable: false,
                        defaultContent: '',
                        width: colWidth,
                        isDynamicColumn: true, // custom attribute to recognized dynamic column container
                        visible: true,  // show all dynamic columns by default - they will be restored according to current scope
                        // FIX: originally this value was set to false, but this coused conditional formatting not to work when loading first time (when row was created cell was not visible)
                        render: $.fn.dataTable.render.cellWrapper(dynamicRender)
                    });
                }
            } else if (settings.dynamicColumns != undefined) {
                throw "Invalid settings.dynamicColumn: expected to be object or undefined";
            }
        }


        // Reveal static method
        $.fn.DataTable.DynamicColumns = {};
        $.fn.DataTable.DynamicColumns.mapDynamicColumns = _fnMapDynamicColumns;

        /**
         * Maps dynamic columns its proper state based on current scope locations
         * - initializes column headers (titles)
         * - sets columns visiblity 
         * @@param api { object } - datatables api reference
         */
        function _fnMapDynamicColumns(api) {
            var dynamicColumns = api.columns().filterDynamic();
            var dynColSettings = api.settings()[0].oInit.dynamicColumns;
            if (dynamicColumns.count() == 0) return;

            var dynamicColumnsState = api.state().dynamicColumns || api.state.read().dynamicColumns; // when there are no dynamic columns inside loaded state read them direcly from storage

            var dynColIndex = 0;
            dynamicColumns.every(function () {
                var dynCol = this;
                var dynColVisible = true;
                if (typeof dynColSettings.isEnabled === "function") {
                    if (dynColSettings.isEnabled(dynColIndex)) {
                        var dynColName = dynColSettings.name(dynColIndex);
                        this.isEnabled(true);
                        // todo add check if title is defined and is an function
                        dynCol.title(dynColSettings.title(dynColIndex));
                        dynCol.name(dynColName);

                        // restore columns state (visibility)
                        if (dynamicColumnsState) {
                            var dynColState = dynamicColumnsState[dynColName];
                            if (dynColState && dynColState.visible === false) {
                                dynColVisible = false;
                            }
                        }

                        dynCol.visible(dynColVisible);
                    } else {
                        this.isEnabled(false);
                    }
                } else {
                    throw "Please specify 'isEnabled' function inside dynamicColumns setting.";
                }
                dynColIndex++;
            });
            api.state.save();
        }

        /**
         * Prepares state object to be saved (extends original state logic)
         * @param {any} api
         * @param {any} data
         */
        function _fnStateSaveHandler(api, data) {
            // save dynamic columns state in seperate property

            // load recent denamic columns settings so that could be merged with current;
            var state = api.state();
            if (!state) {
                state = api.state.read();
            }
            if (state && state.dynamicColumns) {
                // when state is present merge it with state to be saved
                data.dynamicColumns = state.dynamicColumns;
            } else {
                data.dynamicColumns = {}; // add empty storage container
            }

            // overwrite settings for currenty mappped dynamic columns that are in use
            // not used dynamic columns names are prefixed with _
            api.columns().filterDynamic().every(function () {
                var col = this;
                var colName = col.name();
                var isMapped = col.isEnabled();
                if (isMapped) {
                    data.dynamicColumns[colName] = {
                        visible: col.visible()
                    }
                }
            });
        }

        function _fnCreatedRowHandler(settings, baseCreatedRowHandler) {                       
            // use different handlers depending settings configuration (for not configured options handlers will return "empty" function)
            var rowLinesHandler = setRowLines(settings);
            var baseHandler = setBaseHandler(baseCreatedRowHandler);
            return function (row, data, index) {
                var $row = $(row);
                rowLinesHandler($row, data);
                transferCssClass($row);
                baseHandler(row, data, index);
            }
            
            // set row lines attribute based on number of array items inside indicator column.
            function setRowLines(settings) {
                if (settings.rowLines > 0 && settings.rowLinesIndicator !== undefined) {
                    var indicator = settings.rowLinesIndicator;
                    return function ($row, data) {
                        var rowLines = data[indicator].length; // sales order count determines number of lines for whole row.
                        $row.attr('data-row-lines', rowLines);
                    };
                } else {
                    return function () { };
                }
            }
            // conditional cell formatting copy css classes to parent
            function transferCssClass($row) {
                $row.find('.cell[data-parent-css-class]').each(function () {
                    var target = $(this);
                    var cssClass = target.attr('data-parent-css-class');
                    target.parent().addClass(cssClass);
                });
            }
            function setBaseHandler(handler) {
                if (typeof handler === 'function') {
                    return handler; // pass base handler to be executed
                } else {
                    return function () { } // do nothing function;
                }
            }
        }

        /**
         * Sets fixed header width relatively to scroll container
         */
        function _fnSetFixedHeaderWidth() {
            var container = $('#scrollContainer');
            $('#fixedHeader').css('width', container[0].clientWidth);
        }
        /**
         * Sets table body offset relatively to fixed header
         */
        function _fnSetBodyHeaderOffset() {
            $('#scrollContainer').css('padding-top', $('#fixedHeader').height());
        }

        function _htmlEncode(value) {
            if (typeof value != "string") {
                return value;
            }
            return value.replace(/&/g, "&amp;").replace(/>/g, "&gt;").replace(/</g, "&lt;").replace(/"/g, "&quot;");
        }

        /**
            * Gets object property by property path.
            *  * https://stackoverflow.com/questions/6491463/accessing-nested-javascript-objects-with-string-key
            * @param {object} obj - the object which property will be obtained
            * @param {string} path - the object's property path, ex.: .test.test1.test.value
            */
        function _getObjectProperty(obj, path) {
            var output = obj;
            if (path === '') return output;
            path = path.replace(/\[(\w+)\]/g, '.$1'); // convert indexes to properties
            path = path.replace(/^\./, '');           // strip a leading dot
            var a = path.split('.');
            for (var i = 0, n = a.length; i < n; ++i) {
                var k = a[i];
                output = output[k];
            }
            return output;
        }

        /******************************
         * API EXTENSION METHODS 
         * ****************************/

        // Datatables custom extension that enables dynamic column width setting for fixed values.
        $.fn.dataTable.Api.register('column().width()', function (value) {
            // refer to column header element and col element
            var $header = $(this.header());
            var $column = $('col:eq(' + this.index() + ')', this.table().node()); // col element in not standard part of datatables and it is created during table once table is initialized

            // the setter
            if (value != undefined) { // TODO: preform set only when value has changed.
                // store value iniside meta 'data' for column
                // this is primary storage for column width value
                $column.data('xt-width', parseInt(value));
                // individual column width is set to control cells and table width:
                $column.add($header).css('width', value)
                // when column size changes recalculate table width
                this.setWidth();
            }
            // the getter
            else {
                // read value stored inside meta data
                var expectedWidth = $column.data('xt-width')
                if (expectedWidth == undefined) {
                    // when no value stored try to read it form table settings
                    var expectedWidth = parseInt(this.settings()[0].aoColumns[this.index()].width);
                    // and save it as current column width value
                    $column.data('xt-width', expectedWidth);
                }

                return expectedWidth;
            }
            return this;
        });
        //  Datatables custom extension that sets table width according to current column widths
        $.fn.dataTable.Api.register('setWidth()', function () {
            var tableWidth = 0;
            this.columns(':visible').every(function () {
                var currentWidth = this.width();
                tableWidth += currentWidth || 100;
            });
            // apply static width for table header and body that are wrapped iniside separeate tables
            var $wrapper = $(this.table().container());
            $wrapper.find('table').width(tableWidth);
            return this;
        });
        // Datatables custom extension that enables changing column title on runtime.
        $.fn.dataTable.Api.register('column().title()', function (value) {
            //TODO: possibly should update .settings()[0].aoColumns[17].title and .sTitle

            // refer to column header element
            var header = $(this.header());

            // the setter
            if (value != undefined) {
                // store set iniside meta 'data' for header
                // this is primary storage for column width value
                header.text(value);
            }
            // the getter
            else {
                return header.text();
            }
            return this;
        });
        // Datatables custom extension enables table state restoration
        $.fn.dataTable.Api.register('restoreState()', function (stateObject) {
            // calculate page from given state
            var page = (stateObject.start / stateObject.length);
            var order = stateObject.order;
            var redrawRequired = false;

            // navigate to page
            if (this.page() != page) {
                this.page(page).draw('page');
            }
            // apply sort
            if (JSON.stringify(order) != JSON.stringify(this.order())) {
                this.order(order);
                this.redrawRequired = true;
            }
            // redraw table
            if (redrawRequired) {
                this.draw();
            }

            return this;
        });
        // Datatables custom extension that enables to specify default table row height.
        // value is initialized only once form [data-row-lines] attribute placed on table element, then cache is used.
        $.fn.dataTable.Api.register('rowLines()', function (value) {
            var table = $(this.table().node());
            if (value != undefined) {
                if ([0, 1, 2, 3, 4].indexOf(value) >= 0) {
                    table.data('rowLines', value);
                    table.attr('data-row-lines', value);
                } else {
                    throw "Datatable rowLines value is out of range (0 to 4)";
                }
                return this;
            }
            var lines = table.data('rowLines');
            if (!lines) {
                var parsedLines = parseInt(table.attr('data-row-lines'));
                lines = parsedLines === NaN ? 1 : parsedLines;
                table.data('rowLines', lines); // cache attribute value
            }
            return lines;
        });

        // TODO: provide comment
        $.fn.dataTable.Api.register('columns().filterDynamic()', function () {
            var table = $(this.table().node());
            //var columnDefs = this.settings()[0].oInit.columns;
            var api = this;
            var result = this;
            result['0'] = [];
            var cache = table.data('dynamicColumns');
            if (!cache) {
                api.columns().every(function () {
                    var col = this;
                    if (col.isDynamic()) {
                        result['0'].push(col.index());
                    }
                });
                table.data('dynamicColumns', result['0']);
            } else {
                result['0'] = cache;
            }

            return result;
        });
        // TODO: provide comment
        $.fn.dataTable.Api.register('columns().filterEnabled()', function () {
            var table = $(this.table().node());
            var api = this;
            var result = this;
            result['0'] = [];
            // cache does not work well when enabled columns change at runtime (cache is not updated);
            //var cache = table.data('enabledColumns');
            //if (!cache) {
                api.columns().every(function () {
                    var col = this;
                    if (col.isEnabled()) {
                        result['0'].push(col.index());
                    }
                });
                table.data('enabledColumns', result['0']);
            //} else {
            //    result['0'] = cache;
            //}

            return result;
        });
        // TODO: provide comment
        $.fn.dataTable.Api.register('columns().byName()', function (columnName) {
            var api = this;
            var result = undefined;
            api.columns().every(function () {
                var col = this;
                var colName = col.name()
                if (colName === columnName || colName === '_' + columnName) {
                    result = col;
                }
            });
            return result;
        });
        // TODO: provide comment
        $.fn.dataTable.Api.register('dynamicColumn()', function (index) {
            var api = this;
            var dynamicColumns = api.columns().filterDynamic()['0'];
            var mappedColumn = dynamicColumns[index];

            if (mappedColumn >= 0) {
                return api.column(mappedColumn);
            }
            return undefined;
        });
        $.fn.dataTable.Api.register('column().isDynamic()', function () {
            return this.settings()[0].aoColumns[this.index()].isDynamicColumn || false;
        });
        $.fn.dataTable.Api.register('column().isEnabled()', function (value) {
            if (value != undefined) {
                if (value === false) {
                    if (this.isEnabled()) {
                        this.name('_' + this.name());
                    }
                    if (this.visible()) {
                        this.visible(false);
                    }
                    //this.name('_name' + this.index()).visible(false);
                } else if (value === true) {
                    if (!this.isEnabled()) {
                        this.name(this.name().substr(1));
                    }
                    //throw "To enable column change its .name() to non starting with _";
                } else {
                    throw "Disallowed value: " + value;
                }
            } else {
                // columns which name starts with underline is treated as disabled
                return this.name().substr(0, 1) != '_';
            }
            return this;
        });
        $.fn.dataTable.Api.register('column().name()', function (name) {
            var api = this;

            if (name) {
                api.settings()[0].aoColumns[this.index()].sName = name;
            } else {
                return api.settings()[0].aoColumns[this.index()].sName;
            }
            return this;
        });
        $.fn.dataTable.Api.register('column().filterCondition()', function (filterFunction) {
            var api = this;

            if (filterFunction) {
                api.settings()[0].aoColumns[this.index()].filter = filterFunction
            } else {
                return api.settings()[0].aoColumns[this.index()].filterCondition;
            }
            return this;
        });
        $.fn.dataTable.Api.register('filterApply()', function (forceDraw) {
            var activeFilterConditions = [];
            var tableApi = this;

            // check if filter has changed if so prepare new filtering conditions
            tableApi.columns().every(function () {
                var col = this;
                var dataSrc = col.dataSrc();
                var resolver = col.filterCondition();
                if (typeof resolver === 'function') {
                    var condition = resolver();
                    if (typeof condition === 'function') {
                        activeFilterConditions.push(function (settings, searchData, index, rowData, counter) {
                            return condition(rowData[dataSrc]);
                        });
                    }
                }
            });

            // reset previous filters
            $.fn.dataTable.ext.search = [];
            // concentrate all conditions into one function.
            if (activeFilterConditions.length > 0) {
                $.fn.dataTable.ext.search.push(function (settings, searchData, index, rowData, counter) {
                    var conditionsLen = activeFilterConditions.length;
                    var result = true;
                    for (var i = 0; i < conditionsLen; i++) {
                        result = activeFilterConditions[i](settings, searchData, index, rowData, counter);
                        if (result === false) {
                            break;
                        }
                    }
                    return result;
                });
            }

            if (forceDraw) {
                tableApi.draw();
            }
            return tableApi;
        });
        /**
         * returns datatable state directly from storage;
         * @@returnEmptyObject bool - when true returns {} when there's no state saved. When flase returns null when there's no state saved
         */
        $.fn.dataTable.Api.register('state.read()', function (returnEmptyObject) {
            var api = this;
            var settings = api.settings()[0];
            var result = {};

            result = JSON.parse(
                (settings.iStateDuration === -1 ? window.sessionStorage : window.localStorage)
                    .getItem(
                        'DataTables_' + settings.sInstance + '_' + location.pathname
                    )
            )
            if (!result && returnEmptyObject) {
                result = {};
            }
            return result;
        });


        /******************************
         * FILTER HELPERS
         * ****************************/

        // FILTERING:
        // the concept of .filter helpers is to return smallest required condition function than allows to evaluate filter
        // filter conditions are set each time .fillterApply() function is called so they can be optimized against current filter input value:
        // for example: 
        //      for date range filtering if start date is not set optimal condition function will be: return columnData <= dateEnd. 
        //      same result will give less performat function that checks for dateEnd inside condition function: return columnData <
        // adding filter namespace and predefined filtering functions
        $.fn.dataTable.filter = $.fn.dataTable.filter || {};

        // match only not empty values ('', null, undefined, 0 - are considered to be empty)
        $.fn.dataTable.filter.nonEmpty = function (input, options) {
            return function (columnData) {
                return (columnData || undefined) !== undefined;
            }
        };
        $.fn.dataTable.filter.textMatchDefault = function (input) {
            // when (no filter input specified) disable filter condition for this column.
            if ((input || undefined) === undefined) {
                return undefined;
            } else if (input === '^') {
                // when special character ^ specified filter non empty values
                return $.fn.dataTable.filter.nonEmpty();
            } else if ((input || '').length > 0) {
                return $.fn.dataTable.filter.textMatch(input, { mode: 'contains' });
            }            
        }
        // text match with options (case insensitive)
        $.fn.dataTable.filter.textMatch = function (input, options) {
            var opt = $.extend({
                mode: 'contains',
                propertyPath: ''
            }, options || {})

            var mode = opt.mode.toLowerCase();
            var propertyPath = opt.propertyPath || '';

            // when (no filter input specified) disable filter condition for this column.
            if ((input || undefined) === undefined) {
                return undefined;
            } else if (input === '^') {
                // when special character ^ specified filter non empty values
                return $.fn.dataTable.filter.nonEmpty();
            } else if ((input || '').length > 0) {
                //return $.fn.dataTable.filter.textMatch(input, { mode: 'contains' });
                var parsedInput = input.toLowerCase();

                //if (propertyPath != '' && ['contains','exact'].indexOf(mode) === 1) {
                //    throw 'Not implemented: propertyPath implemented only for "contains" mode!'
                //}

                // assuming input is checked for emptiness outside procedure
                if (mode == 'commaSeparated'
                    || mode == 'comma'
                    || mode == 'split'
                    || mode == 'fragments'
                    || mode == 'commaSep') {
                    // when not 0 length text specified
                    // split comma separated values
                    var fragments = parsedInput.split(",").map(function (fragment) {
                        var trimmed = fragment.trim();
                        if (trimmed.length > 0) {
                            return trimmed; // take only non empty values
                        }
                    });
                    return function (columnData) {
                        // filter rows that match any of the fragments
                        return fragments.indexOf(columnData) !== -1;
                    }
                } else if (mode == 'contains') {
                    var condition = function (columnData) {
                        // filter rows that contain
                        return columnData.toLowerCase().indexOf(parsedInput) !== -1;
                    }
                    return getTextFilter(condition, propertyPath);
                } else if (mode == 'startswith') {
                    return function (columnData) {
                        // filter rows that starts with
                        return columnData.toLowerCase().indexOf(parsedInput) === 0;
                    }
                    return getTextFilter(condition, propertyPath);
                } else if (mode == 'exact') { // exact match
                    var condition = function (columnData) {
                        // filter rows that have value different than filter;
                        return columnData.toLowerCase() === parsedInput;
                    }
                    return getTextFilter(condition, propertyPath);
                    //return function (columnData) {
                    //    // filter rows that starts with
                    //    return columnData.toLowerCase() === parsedInput;
                    //}
                } else {
                    throw "Mode '" + mode + "' is not implemented!";
                }

                function getTextFilter(condition, propertyPath) {
                    if (propertyPath.indexOf('[]') === -1) {
                        // non array comparsion
                        return condition
                    } else {
                        // array comparsion
                        return $.fn.dataTable.filter.arrayCompare(condition, propertyPath.substr(2));
                    }
                }
            } 
        };
        $.fn.dataTable.filter.intMatch = function (input, options) {
            var opt = $.extend({
                mode: 'equal'
            }, options || {});

            var mode = opt.mode.toLowerCase();
            // skip filtering non int values
            if (typeof input === 'string' && !isInt(input)) {
                return undefined;
            }
            var parsedInput = null;
            if (input !== null) {
                parsedInput = parseInt(input);
            }
            

            if (mode == '>'
                || mode == 'largerthen') {
                return function (columnData) {
                    return columnData > parsedInput;
                };
            } else { // exact match
                return function (columnData) {
                    return columnData === parsedInput;
                };
            }

            //https://stackoverflow.com/questions/175739/built-in-way-in-javascript-to-check-if-a-string-is-a-valid-number/24457420#24457420
            function isInt(value) {
                return /^-{0,1}\d+$/.test(value);
            }
        }
        $.fn.dataTable.filter.dateRange = function (dateStart, dateEnd, options) {
            // to keep standard
            var opt = $.extend({
                mode: 'between',
                propertyPath: '', // '' - compare value, [] - iterate and compare array values, [].prop - iterate and compare array values with nested property
                compareDateOnly: true // when true filter input value time will be reduced to 00:00:00
            }, options || {});

            var hasDateStart = (dateStart || undefined) !== undefined;
            var hasDateEnd = (dateEnd || undefined) !== undefined;
            var propertyPath = opt.propertyPath || '';

            var dateStart = dateStart;
            var dateEnd = dateEnd;

            if (opt.compareDateOnly) {
                if (hasDateStart) dateStart.setHours(0, 0, 0, 0);
                if (hasDateEnd) dateEnd.setHours(23, 59, 59, 999);
            }

            // assuming input is proper date
            if (!hasDateStart && !hasDateEnd) {
                return undefined;
            } else if ( hasDateStart && !hasDateEnd ) {
                // larger than dateStart
                var condition = function (columnData) {
                    return columnData >= dateStart
                }
                if (propertyPath.indexOf('[]') === -1) {
                    // non array comparsion
                    return condition
                } else {
                    // array comparsion
                    return $.fn.dataTable.filter.arrayCompare(condition, propertyPath.substr(2));
                }
            } else if ( !hasDateStart && hasDateEnd ) {
                // lower than dateEnd
                var condition = function (columnData) {
                    return columnData <= dateEnd
                }
                if (propertyPath.indexOf('[]') === -1) {
                    // non array comparsion
                    return condition
                } else {
                    // array comparsion
                    return $.fn.dataTable.filter.arrayCompare(condition, propertyPath.substr(2));
                }
            } else {
                // between
                var condition = function (columnData) {
                    return columnData >= dateStart && columnData <= dateEnd;
                }
                if (propertyPath.indexOf('[]') === -1) {
                    // non array comparsion
                    return condition
                } else {
                    // array comparsion
                    return $.fn.dataTable.filter.arrayCompare(condition, propertyPath.substr(2));
                }
            }
        }

        // array comparer:
        // checks array values against condition function - first met condition satisfies whole array check
        $.fn.dataTable.filter.arrayCompare = function(condition, path) {
            if ((path || undefined) === undefined) {
                return function (columnData) {
                    for (var i = 0; i < columnData.length; i++) {
                        var value = columnData[i];
                        if (condition(value)) {
                            return true;
                        }
                    }
                    return false;
                }
            } else {
                return function (columnData) {
                    for (var i = 0; i < columnData.length; i++) {
                        var value = _getObjectProperty(columnData[i], path);
                        if (condition(value)) {
                            return true;
                        }
                    }
                    return false;
                }
            }
        }

        /******************************
         * SORTING / ORDERING HELPERS 
         * ****************************/
        // docs: https://datatables.net/plug-ins/sorting/

        // sort column value consiting multiple items inside array with lowest item value for asc and highest value for desc for each row.
        function _fnArraySortPreprocess(arr) {

            if (!Array.isArray(arr) && arr !== undefined && arr !== null && arr !== '') {
                throw "array sort function expects input value to be an array, passed value has different type.";
            }
            var result = null;
            if (arr !== undefined && arr !== null && arr.length !== 0 && arr !== '') {
                //console.log("before sort:",formatArray(arr));
                result = sortByType(arr);
                //console.log("after sort:",formatArray(result));
            }
            return result;

            function sortByType(arr) {
                if (arr[0] instanceof Date) { // assuming whole array is same type
                    return arr.sort(function (a, b) {
                        if (a === b) { return 0; };
                        return a.getTime() - b.getTime();
                    });
                } else if (typeof arr[0] === "string") {
                    return arr.sort(function (a, b) {
                        if (a === b) { return 0; };
                        return ('' + a).localeCompare(b);
                    });
                } else {
                    return arr.sort();
                }
            }
        }
        //function formatArray(array) {
        //    if (array !== null && array !== undefined) {
        //        return array.map(function (val) {
        //            return consoleFormat(val);
        //        });
        //    }
        //}
        //function consoleFormat(val) {
        //    if (val instanceof Date) {
        //        return moment(val).format('YYYY-MM-DD')
        //    }
        //    return val;
        //}
        jQuery.extend(jQuery.fn.dataTableExt.oSort, {
            //'array-pre': function (arr) {
            //    return _fnArraySortPreprocess(arr);
            //},
            "array-asc": function (valA, valB) {
                var a = _fnArraySortPreprocess(valA);
                var b = _fnArraySortPreprocess(valB);
                //console.log("asc preVal", formatArray(a), formatArray(b));
                if (a === null && b === null) {
                    return 0;
                } else if (b === null) {
                    return 1;
                } else if (a === null) {
                    return -1;
                }
                var result = 0;
                var minA = a[0];
                var minB = b[0];
                //console.log("minVal", consoleFormat(minA), consoleFormat(minB));

                if (minA === minB) {
                    return result;
                } else if (typeof minA === "string") {
                    result = ('' + minA).localeCompare(minB);
                } else if (minA instanceof Date) {
                    result = minA.getTime() - minB.getTime();
                } else {
                    result = minA < minB ? -1 : 1;
                }
                return result;                
            },
            "array-desc": function (valA, valB) {
                var a = _fnArraySortPreprocess(valA);
                var b = _fnArraySortPreprocess(valB);
                
                //console.log("desc: preVal", formatArray(a), formatArray(b));
                if (a === null && b === null) {
                    return 0;
                } else if (b === null) {
                    return -1;
                } else if (a === null) {
                    return 1;
                }

                var result = 0
                var maxA = a[a.length - 1];
                var maxB = b[b.length - 1];
                //console.log("maxVal", consoleFormat(maxA), consoleFormat(maxB));

                if (maxA === maxB) {
                    return result;
                } else if (typeof maxA === "string") {
                    result = -('' + maxA).localeCompare(maxB);
                } else if (maxA instanceof Date) {
                    result = -(maxA.getTime() - maxB.getTime());
                } else {
                    result = maxA > maxB ? 1 : -1;
                }

                return result;     
            },
        });


        /******************************
         * RENDER HELPERS 
         * ****************************/

        // dataTableCellWrapper function enables table cell content wrapping with extra div
        // that allows additional features like:
        // - cell background setting (without padding).
        // - set cell fixed height to control table row height;
        // - html ecoding control
        // baseRenderer - optional function that will be used to render value.
        // see: https://datatables.net/reference/option/columns.render
        $.fn.dataTable.render.cellWrapper = function(baseRenderer) {
            return function (data, type, row, meta) {
                var result = null;
                var renderOutput = {};
                var colSettings = meta.settings.oInit.columns[meta.col];
                if (typeof baseRenderer == "function") {
                    // execute custom renderer if specified.
                    // CAUTION: base renderer should take care about encoding
                    renderOutput = baseRenderer(data, type, row, meta);
                    // when renderOutput is and object and .result is present treat is as result.
                    if (renderOutput !== null && typeof renderOutput  === "object" && renderOutput.hasOwnProperty('result')) {
                        result = renderOutput.result;
                        if (type === 'display' && result === null) {
                            result = colSettings.defaultContent; // use defaults for null values;
                        }
                    } else {
                        result = renderOutput;
                        renderOutput = {}; // reset render output settings for compatiblity => will be able to preform checks like... if (renderOutput.tooltip)
                        if (type === 'display' && result === null) {
                            result = colSettings.defaultContent; // use defaults for null values;
                        }
                        if (type !== 'display') {
                            // when type of render is other than "display" return raw result
                            return result;
                        }
                    }
                } else if (typeof baseRenderer == "object" && typeof baseRenderer.display == "function") {
                    result = baseRenderer.display(data, type, row, meta);
                } else if (type == "display") {
                    // when null return defaultContent from table settings
                    if (data != null) {
                        result = _htmlEncode(data);
                    }
                    else {
                        result = colSettings.defaultContent;
                    }
                } else {
                    // for types other than display return original data value.
                    return data;
                }
                var toolTipElement = '';
                var rowHeightIndicator = '';
                var expander = '';
                var cssClass = renderOutput.cssClass || '';
                var parentCssClass = '';
                var editorMeta = '';
                var dataAttributes = '';
                if (colSettings.isHeightIndicator) {
                    rowHeightIndicator = 'height-indicator'
                }
                if (renderOutput.tooltip) {
                    var width = renderOutput.tooltipWidth ? 'style="width:' + renderOutput.tooltipWidth + 'px"' : '';
                    toolTipElement = '<div class="cell-tip" ' + width + '>' + renderOutput.tooltip + '</div>'
                }
                if (renderOutput.enableExpander) {
                    expander = '<a href="#" class="cell-expander" title="toggle more/less"></a>'
                }
                if (renderOutput.parentCssClass) {
                    parentCssClass = ' data-parent-css-class="' + renderOutput.parentCssClass + '"';
                }
                if (renderOutput.dataAttributes) {
                    renderOutput.dataAttributes.map(function (attr) {
                        if (attr.value !== undefined && attr.value !== null) {
                            dataAttributes += " " + attr.name + "='" + attr.value + "'";
                        }
                    });
                }
                var action = renderOutput.action ? 'data-action="' + renderOutput.action + '"' : '';
                var editor = colSettings.editor;
                if (editor) {
                    if (renderOutput.editor === undefined || renderOutput.editor === true) {
                        editorMeta = ' data-edit="' + (editor.type || 'text') + '"';
                    }
                }

                // data-parent-css-class will be copied to class attribute of TD element
                var output = expander + toolTipElement + '<div class="cell ' + rowHeightIndicator + ' ' + cssClass + '" ' + action + parentCssClass + editorMeta + dataAttributes + '>' + result + '</div>';

                //console.log(output);
                return output;
            }
        }

        /******************************
         * Utils
         * ****************************/

        $.fn.dataTable.utils = $.fn.dataTable.utils || {};
        $.fn.dataTable.utils.htmlEncode = _htmlEncode;
}));


