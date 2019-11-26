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

         // *** INITIALIZATION ***
        // When any of the tables on the page initializes
        $(document).on('init.dt', function (e, settings, json) {
            if (e.namespace !== 'dt') {
                return;
            }
            var api = new $.fn.dataTable.Api(settings);
            var $table = $(api.table().node());
            var dataTableSettings = settings;

            // INLINE EDTIOR
            // edition activation
            $table.on('click', '[data-edit]', function () {
                var $td = $(this).closest('td');                
                var editedValue = api.cell($td).data();
                var cell = api.cell($td);
                var editorSettings = api.init().columns[cell.index().column].editor;

                if (editorSettings.type == 'text') {
                    $td.append('<textarea class="cell-editor">' + editedValue + '</textarea>');

                } else if (editorSettings.type == 'select') {
                    var selectHtml = '<select class="cell-editor" value="' + editedValue + '" size="8" >';
                    editorSettings.options.map(function (option) {
                        var selectedMeta = option.value + '' === editedValue + '' ? 'selected="selected"' : ''; // TODO: values are compared as string that my not be the best solution in general
                        selectHtml += '<option value="' + option.value + '" ' + selectedMeta + '>' + option.text + '</option>';
                    });
                    selectHtml += '</select>';
                    $td.append(selectHtml);

                } else if (editorSettings.type == 'tags') {
                    console.log(editedValue);
                    var editedValue = editedValue || [];
                    var placeholder = "";
                    var editorLanguage = settings.oLanguage.editor
                    if (editorLanguage && editorLanguage.tagsPlaceholder) {
                        placeholder = editorLanguage.tagsPlaceholder
                    }
                    var tagsHtml = '<div class="cell-editor hidden"><input name="tags" data-tags placeholder="' + placeholder + '"></input></div>';
                    $editor = $(tagsHtml).appendTo($td);
                    var tagify = new Tagify($editor.find('[data-tags]')[0], {
                        whitelist: [''],
                        dropdown: {
                            classname: "",
                            enabled: 2,
                            maxItems: 5
                        }
                    });

                    tagify.addTags(editedValue.join());
                    var widget = $editor;
                    widget.data('tagify', tagify);

                    setTimeout(function () {
                        $editor.removeClass('hidden');
                        widget.focusArea({
                            focusTarget: $('.cell-editor .tagify__input')
                        }).blur(function (widget) {
                            var tagify = widget.data('tagify');
                            var tagList = tagify.getTagValues();
                            _fnEditorCommitEdition.call(widget, false, tagList);
                        });
                    }, 300);
                } else {
                    throw 'Unsupported inline editor type "' + editorSettings.type + '"';
                }

                var $editor = $('.cell-editor'); // refernce edit element
                $editor.focus();
                $editor.data('value', JSON.stringify(editedValue)); // save original value for dirty checking
                $editor.on('keydown', function (e) {
                    // Submit form when pressing SHIFT + ENTER
                    if (e.keyCode === 13 && e.shiftKey) {
                        $editor = $(this);
                        if ($editor.is('select') || $editor.is('textarea')) {
                            _fnEditorCommitEdition.call(this, false, $editor.val());
                        } else if ($editor.find('[data-tags]').length > 0) {
                            var tagify = widget.data('tagify');
                            setTimeout(function () { // delay for tagify tag add processing.
                                var tagList = tagify.getTagValues();
                                _fnEditorCommitEdition.call(widget, false, tagList);
                            }, 300);
                        }
                        e.preventDefault();
                    }
                    if (e.keyCode === 27) {
                        $td.removeClass('edit-on');
                        $editor.remove();
                        e.preventDefault();
                    }
                });
                // save changes on editor deactivation
                $editor.filter('select').on('change', function () {
                    _fnEditorCommitEdition.call(this, false, $(this).val());
                });
                $editor.filter('select').on('blur', function () {
                    _fnEditorCommitEdition.call(this, true, $(this).val());
                });
                $editor.filter('textarea').on('blur', function () {
                    _fnEditorCommitEdition.call(this, false, $(this).val());
                });


                $td.addClass('edit-on');

                return false;

            });
            /** Handles inline editor finalization  */
            function _fnEditorCommitEdition(skipSave, rawValue) {
                var $editor = $(this);
                var $td = $editor.parent();
                var cell = api.cell($td);
                var rowData = api.row(cell.index().row).data();
                var editorSettings = api.init().columns[cell.index().column].editor;
                var editedValue = parseValue(rawValue);

                if (skipSave === true) {
                    endEdit();
                    return;
                }

                //check if update is needed
                if ($editor.data('value') == JSON.stringify(editedValue)) {
                    // when no change skip without saving
                    endEdit()
                    return;
                }

                var e = $.Event('editor-beforeSave.dt');
                $table.trigger(e, [editedValue, rowData, api]);

                editorSettings.onSave(editedValue, rowData,
                    function (response) {
                        var e = $.Event('editor-saveSuccess.dt');
                        $table.trigger(e, [response, editedValue, rowData, api]);

                        endEdit();
                        cell.data(editedValue); 
                    },
                    function (response) {
                        endEdit();
                        var e = $.Event('editor-saveError.dt');
                        $table.trigger(e, [response, editedValue, rowData, api]);
                    });

                function endEdit() {
                    // ISSUE: DOMException: Failed to execute 'removeChild' on 'Node': The node to be removed is no longer a child of this node. Perhaps it was moved in a 'blur' event handler?
                    // FIX: dalay remove exection so that DOM background events may be handled first
                    setTimeout(function () {
                        $editor.remove();
                        $td.removeClass('edit-on');
                    }, 1);
                }

                /** Gets edited value preforming proper type cast.
                 * */
                function parseValue(rawValue) {
                    var unparsedValue = rawValue
                    var value = unparsedValue == 'null' ? null : unparsedValue;
                    var valueType = editorSettings.valueType;
                    if (valueType && valueType.toLowerCase() != 'string' && value != null) {
                        if (valueType.toLowerCase() == 'int') {
                            return parseInt(value);
                        }
                        // other type conversions can be implemented here;

                        throw "editorCommitEdition(): Unsupported data type '" + valueType + "'";
                    }
                    return value;
                }
            }
        });
}));


