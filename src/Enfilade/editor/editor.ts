import {basicSetup} from "codemirror";
import {python} from "@codemirror/lang-python";
import {Annotation, Compartment, EditorState, type Extension} from "@codemirror/state";
import {EditorView, keymap} from "@codemirror/view";
import {HighlightStyle, syntaxHighlighting} from "@codemirror/language";
import {indentWithTab} from "@codemirror/commands";
import {tags} from "@lezer/highlight";

interface DotNetReference {
    invokeMethodAsync(method: string): Promise<void>;

    dispose(): void;
}

const external = Annotation.define<boolean>();
const editable = new Compartment();
const views = new WeakMap<HTMLElement, EditorView>();

const wash = "color-mix(in srgb, var(--klein) 12%, transparent)";

const theme = EditorView.theme(
    {
        "&": {color: "var(--light)", backgroundColor: "transparent", height: "100%", fontSize: "13px"},
        "&.cm-focused": {outline: "none"},
        ".cm-scroller": {
            fontFamily: "var(--mono)",
            fontStretch: "87%",
            letterSpacing: "-0.01em",
            lineHeight: "20px",
            scrollbarWidth: "thin",
            scrollbarColor: "var(--rule) transparent"
        },
        ".cm-content": {caretColor: "var(--glow)", padding: "16px 0"},
        ".cm-line": {padding: "0 20px 0 16px"},
        ".cm-cursor": {borderLeftColor: "var(--glow)", borderLeftWidth: "2px"},
        ".cm-gutters": {
            backgroundColor: "transparent",
            color: "var(--line)",
            border: "none",
            borderRight: "1px solid var(--rule)"
        },
        ".cm-gutter": {minWidth: "40px"},
        ".cm-lineNumbers .cm-gutterElement": {padding: "0 12px 0 16px", fontSize: "12px"},
        ".cm-foldGutter .cm-gutterElement": {padding: "0 6px", color: "var(--line)", cursor: "pointer"},
        ".cm-foldGutter .cm-gutterElement:hover": {color: "var(--light)"},
        ".cm-activeLine": {backgroundColor: wash},
        ".cm-activeLineGutter": {backgroundColor: wash, color: "var(--light)"},
        ".cm-matchingBracket, &.cm-focused .cm-matchingBracket": {
            backgroundColor: "var(--wash)",
            outline: "1px solid var(--line)",
            borderRadius: "3px"
        },
        ".cm-selectionMatch": {backgroundColor: "var(--wash)"},
        "&.cm-focused > .cm-scroller > .cm-selectionLayer .cm-selectionBackground, .cm-selectionBackground, ::selection": {
            backgroundColor: "color-mix(in srgb, var(--klein) 45%, transparent)",
        },
        ".cm-foldPlaceholder": {
            backgroundColor: "var(--wash)",
            border: "none",
            color: "var(--mist)",
            borderRadius: "6px",
            padding: "0 6px"
        },
        ".cm-tooltip": {
            backgroundColor: "var(--deep)",
            color: "var(--light)",
            border: "none",
            borderRadius: "var(--round)",
            boxShadow: "inset 0 0 0 1px var(--rule), var(--lift)",
            overflow: "hidden"
        },
        ".cm-panels": {backgroundColor: "var(--deep)", color: "var(--light)", borderColor: "var(--rule)"},
    },
    {dark: true},
);

const highlight = HighlightStyle.define([
    {tag: [tags.keyword, tags.operatorKeyword, tags.controlKeyword], color: "var(--glow)"},
    {tag: [tags.string, tags.special(tags.string)], color: "var(--ochre)"},
    {tag: [tags.number, tags.bool, tags.null], color: "var(--vermilion)"},
    {tag: [tags.comment, tags.lineComment], color: "var(--mist)", fontStyle: "italic"},
    {
        tag: [tags.function(tags.variableName), tags.definition(tags.variableName)],
        color: "var(--light)",
        fontWeight: "600"
    },
    {tag: tags.meta, color: "var(--glow)"},
]);

const languages: Record<string, () => Extension> = {python};

export function create(element: HTMLElement, dotnet: DotNetReference, content: string, readOnly: boolean, language: string | null): void {
    views.get(element)?.destroy();
    const view = new EditorView({
        parent: element,
        state: EditorState.create({
            doc: content,
            extensions: [
                basicSetup,
                keymap.of([indentWithTab]),
                language ? languages[language]?.() ?? [] : [],
                theme,
                syntaxHighlighting(highlight),
                editable.of(EditorView.editable.of(!readOnly)),
                EditorView.updateListener.of(update => {
                    if (update.docChanged && !update.transactions.some(transaction => transaction.annotation(external)))
                        void dotnet.invokeMethodAsync("EditorChanged");
                }),
            ],
        }),
    });
    views.set(element, view);
}

export function getValue(element: HTMLElement): string {
    return views.get(element)?.state.doc.toString() ?? "";
}

export function setValue(element: HTMLElement, content: string): void {
    const view = views.get(element);
    if (!view) return;
    view.dispatch({
        changes: {from: 0, to: view.state.doc.length, insert: content},
        annotations: external.of(true),
    });
}

export function setReadOnly(element: HTMLElement, readOnly: boolean): void {
    views.get(element)?.dispatch({effects: editable.reconfigure(EditorView.editable.of(!readOnly))});
}

export function dispose(element: HTMLElement): void {
    views.get(element)?.destroy();
    views.delete(element);
}
