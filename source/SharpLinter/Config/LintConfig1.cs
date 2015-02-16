using Newtonsoft.Json;

namespace SharpLinter.Config
{
	internal class LintConfig
	{
		/// <summary>
		/// JSON setting for bitwise.
		/// </summary>
		[JsonProperty("bitwise")]
		public bool Bitwise { get; set; }

		/// <summary>
		/// JSON setting for freeze.
		/// </summary>
		[JsonProperty("freeze")]
		public bool Freeze { get; set; }

		/// <summary>
		/// JSON setting for camelcase.
		/// </summary>
		[JsonProperty("camelcase")]
		public bool Camelcase { get; set; }

		/// <summary>
		/// JSON setting for curly.
		/// </summary>
		[JsonProperty("curly")]
		public bool Curly { get; set; }

		/// <summary>
		/// JSON setting for eqeqeq.
		/// </summary>
		[JsonProperty("eqeqeq")]
		public bool Eqeqeq { get; set; }

		/// <summary>
		/// JSON setting for notypeof.
		/// </summary>
		[JsonProperty("notypeof")]
		public bool Notypeof { get; set; }

		/// <summary>
		/// JSON setting for es3.
		/// </summary>
		[JsonProperty("es3")]
		public bool Es3 { get; set; }

		/// <summary>
		/// JSON setting for es5.
		/// </summary>
		[JsonProperty("es5")]
		public bool Es5 { get; set; }

		/// <summary>
		/// JSON setting for forin.
		/// </summary>
		[JsonProperty("forin")]
		public bool Forin { get; set; }

		/// <summary>
		/// JSON setting for funcscope.
		/// </summary>
		[JsonProperty("funcscope")]
		public bool Funcscope { get; set; }

		/// <summary>
		/// JSON setting for globalstrict.
		/// </summary>
		[JsonProperty("globalstrict")]
		public bool Globalstrict { get; set; }

		/// <summary>
		/// JSON setting for immed.
		/// </summary>
		[JsonProperty("immed")]
		public bool Immed { get; set; }

		/// <summary>
		/// JSON setting for iterator.
		/// </summary>
		[JsonProperty("iterator")]
		public bool Iterator { get; set; }

		/// <summary>
		/// JSON setting for newcap.
		/// </summary>
		[JsonProperty("newcap")]
		public bool Newcap { get; set; }

		/// <summary>
		/// JSON setting for noarg.
		/// </summary>
		[JsonProperty("noarg")]
		public bool Noarg { get; set; }

		/// <summary>
		/// JSON setting for nocomma.
		/// </summary>
		[JsonProperty("nocomma")]
		public bool Nocomma { get; set; }

		/// <summary>
		/// JSON setting for noempty.
		/// </summary>
		[JsonProperty("noempty")]
		public bool Noempty { get; set; }

		/// <summary>
		/// JSON setting for nonbsp.
		/// </summary>
		[JsonProperty("nonbsp")]
		public bool Nonbsp { get; set; }

		/// <summary>
		/// JSON setting for nonew.
		/// </summary>
		[JsonProperty("nonew")]
		public bool Nonew { get; set; }

		/// <summary>
		/// JSON setting for undef.
		/// </summary>
		[JsonProperty("undef")]
		public bool Undef { get; set; }

		/// <summary>
		/// JSON setting for singleGroups.
		/// </summary>
		[JsonProperty("singleGroups")]
		public bool SingleGroups { get; set; }

		/// <summary>
		/// JSON setting for enforceall.
		/// </summary>
		[JsonProperty("enforceall")]
		public bool Enforceall { get; set; }

		/// <summary>
		/// JSON setting for asi.
		/// </summary>
		[JsonProperty("asi")]
		public bool Asi { get; set; }

		/// <summary>
		/// JSON setting for multistr.
		/// </summary>
		[JsonProperty("multistr")]
		public bool Multistr { get; set; }

		/// <summary>
		/// JSON setting for debug.
		/// </summary>
		[JsonProperty("debug")]
		public bool Debug { get; set; }

		/// <summary>
		/// JSON setting for boss.
		/// </summary>
		[JsonProperty("boss")]
		public bool Boss { get; set; }

		/// <summary>
		/// JSON setting for phantom.
		/// </summary>
		[JsonProperty("phantom")]
		public bool Phantom { get; set; }

		/// <summary>
		/// JSON setting for evil.
		/// </summary>
		[JsonProperty("evil")]
		public bool Evil { get; set; }

		/// <summary>
		/// JSON setting for plusplus.
		/// </summary>
		[JsonProperty("plusplus")]
		public bool Plusplus { get; set; }

		/// <summary>
		/// JSON setting for proto.
		/// </summary>
		[JsonProperty("proto")]
		public bool Proto { get; set; }

		/// <summary>
		/// JSON setting for scripturl.
		/// </summary>
		[JsonProperty("scripturl")]
		public bool Scripturl { get; set; }

		/// <summary>
		/// JSON setting for strict.
		/// </summary>
		[JsonProperty("strict")]
		public bool Strict { get; set; }

		/// <summary>
		/// JSON setting for sub.
		/// </summary>
		[JsonProperty("sub")]
		public bool Sub { get; set; }

		/// <summary>
		/// JSON setting for supernew.
		/// </summary>
		[JsonProperty("supernew")]
		public bool Supernew { get; set; }

		/// <summary>
		/// JSON setting for laxbreak.
		/// </summary>
		[JsonProperty("laxbreak")]
		public bool Laxbreak { get; set; }

		/// <summary>
		/// JSON setting for laxcomma.
		/// </summary>
		[JsonProperty("laxcomma")]
		public bool Laxcomma { get; set; }

		/// <summary>
		/// JSON setting for validthis.
		/// </summary>
		[JsonProperty("validthis")]
		public bool Validthis { get; set; }

		/// <summary>
		/// JSON setting for withstmt.
		/// </summary>
		[JsonProperty("withstmt")]
		public bool Withstmt { get; set; }

		/// <summary>
		/// JSON setting for moz.
		/// </summary>
		[JsonProperty("moz")]
		public bool Moz { get; set; }

		/// <summary>
		/// JSON setting for noyield.
		/// </summary>
		[JsonProperty("noyield")]
		public bool Noyield { get; set; }

		/// <summary>
		/// JSON setting for eqnull.
		/// </summary>
		[JsonProperty("eqnull")]
		public bool Eqnull { get; set; }

		/// <summary>
		/// JSON setting for lastsemic.
		/// </summary>
		[JsonProperty("lastsemic")]
		public bool Lastsemic { get; set; }

		/// <summary>
		/// JSON setting for loopfunc.
		/// </summary>
		[JsonProperty("loopfunc")]
		public bool Loopfunc { get; set; }

		/// <summary>
		/// JSON setting for expr.
		/// </summary>
		[JsonProperty("expr")]
		public bool Expr { get; set; }

		/// <summary>
		/// JSON setting for esnext.
		/// </summary>
		[JsonProperty("esnext")]
		public bool Esnext { get; set; }

		/// <summary>
		/// JSON setting for elision.
		/// </summary>
		[JsonProperty("elision")]
		public bool Elision { get; set; }

		/// <summary>
		/// JSON setting for mootools.
		/// </summary>
		[JsonProperty("mootools")]
		public bool Mootools { get; set; }

		/// <summary>
		/// JSON setting for couch.
		/// </summary>
		[JsonProperty("couch")]
		public bool Couch { get; set; }

		/// <summary>
		/// JSON setting for jasmine.
		/// </summary>
		[JsonProperty("jasmine")]
		public bool Jasmine { get; set; }

		/// <summary>
		/// JSON setting for jquery.
		/// </summary>
		[JsonProperty("jquery")]
		public bool Jquery { get; set; }

		/// <summary>
		/// JSON setting for node.
		/// </summary>
		[JsonProperty("node")]
		public bool Node { get; set; }

		/// <summary>
		/// JSON setting for qunit.
		/// </summary>
		[JsonProperty("qunit")]
		public bool Qunit { get; set; }

		/// <summary>
		/// JSON setting for rhino.
		/// </summary>
		[JsonProperty("rhino")]
		public bool Rhino { get; set; }

		/// <summary>
		/// JSON setting for shelljs.
		/// </summary>
		[JsonProperty("shelljs")]
		public bool Shelljs { get; set; }

		/// <summary>
		/// JSON setting for prototypejs.
		/// </summary>
		[JsonProperty("prototypejs")]
		public bool Prototypejs { get; set; }

		/// <summary>
		/// JSON setting for yui.
		/// </summary>
		[JsonProperty("yui")]
		public bool Yui { get; set; }

		/// <summary>
		/// JSON setting for mocha.
		/// </summary>
		[JsonProperty("mocha")]
		public bool Mocha { get; set; }

		/// <summary>
		/// JSON setting for wsh.
		/// </summary>
		[JsonProperty("wsh")]
		public bool Wsh { get; set; }

		/// <summary>
		/// JSON setting for worker.
		/// </summary>
		[JsonProperty("worker")]
		public bool Worker { get; set; }

		/// <summary>
		/// JSON setting for nonstandard.
		/// </summary>
		[JsonProperty("nonstandard")]
		public bool Nonstandard { get; set; }

		/// <summary>
		/// JSON setting for browser.
		/// </summary>
		[JsonProperty("browser")]
		public bool Browser { get; set; }

		/// <summary>
		/// JSON setting for browserify.
		/// </summary>
		[JsonProperty("browserify")]
		public bool Browserify { get; set; }

		/// <summary>
		/// JSON setting for devel.
		/// </summary>
		[JsonProperty("devel")]
		public bool Devel { get; set; }

		/// <summary>
		/// JSON setting for dojo.
		/// </summary>
		[JsonProperty("dojo")]
		public bool Dojo { get; set; }

		/// <summary>
		/// JSON setting for typed.
		/// </summary>
		[JsonProperty("typed")]
		public bool Typed { get; set; }

		/// <summary>
		/// JSON setting for onecase.
		/// </summary>
		[JsonProperty("onecase")]
		public bool Onecase { get; set; }

		/// <summary>
		/// JSON setting for regexp.
		/// </summary>
		[JsonProperty("regexp")]
		public bool Regexp { get; set; }

		/// <summary>
		/// JSON setting for regexdash.
		/// </summary>
		[JsonProperty("regexdash")]
		public bool Regexdash { get; set; }

		/// <summary>
		/// JSON setting for maxlen.
		/// </summary>
		[JsonProperty("maxlen")]
		public System.Int32 Maxlen { get; set; }

		/// <summary>
		/// JSON setting for indent.
		/// </summary>
		[JsonProperty("indent")]
		public System.Int32 Indent { get; set; }

		/// <summary>
		/// JSON setting for maxerr.
		/// </summary>
		[JsonProperty("maxerr")]
		public System.Int32 Maxerr { get; set; }

		/// <summary>
		/// JSON setting for predef.
		/// </summary>
		[JsonProperty("predef")]
		public bool Predef { get; set; }

		/// <summary>
		/// JSON setting for globals.
		/// </summary>
		[JsonProperty("globals")]
		public System.String[] Globals { get; set; }

		/// <summary>
		/// JSON setting for quotmark.
		/// </summary>
		[JsonProperty("quotmark")]
		public System.String Quotmark { get; set; }

		/// <summary>
		/// JSON setting for scope.
		/// </summary>
		[JsonProperty("scope")]
		public bool Scope { get; set; }

		/// <summary>
		/// JSON setting for maxstatements.
		/// </summary>
		[JsonProperty("maxstatements")]
		public System.Int32 Maxstatements { get; set; }

		/// <summary>
		/// JSON setting for maxdepth.
		/// </summary>
		[JsonProperty("maxdepth")]
		public System.Int32 Maxdepth { get; set; }

		/// <summary>
		/// JSON setting for maxparams.
		/// </summary>
		[JsonProperty("maxparams")]
		public System.Int32 Maxparams { get; set; }

		/// <summary>
		/// JSON setting for maxcomplexity.
		/// </summary>
		[JsonProperty("maxcomplexity")]
		public System.Int32 Maxcomplexity { get; set; }

		/// <summary>
		/// JSON setting for shadow.
		/// </summary>
		[JsonProperty("shadow")]
		public System.String Shadow { get; set; }

		/// <summary>
		/// JSON setting for unused.
		/// </summary>
		[JsonProperty("unused")]
		public System.String Unused { get; set; }

		/// <summary>
		/// JSON setting for latedef.
		/// </summary>
		[JsonProperty("latedef")]
		public System.String Latedef { get; set; }

		/// <summary>
		/// JSON setting for ignore.
		/// </summary>
		[JsonProperty("ignore")]
		public bool Ignore { get; set; }

		/// <summary>
		/// JSON setting for ignoreDelimiters.
		/// </summary>
		[JsonProperty("ignoreDelimiters")]
		public bool IgnoreDelimiters { get; set; }

	}
}
