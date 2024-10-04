using System;
using System.IO;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	
	public class GitHistoryParserTest
	{
		const string oneEntryLog = "Commit:24024978a9823df37a23afe533ad1c81f62467ed\nTime:2009-06-13 10:37:42 +0000\nAuthor:cj_sutherland\nE-Mail:cj_sutherland@8a0e9b86-a613-0410-befa-817088176213\nMessage:Expanded the tests to also cover default values.\n\ngit-svn-id: https://ccnet.svn.sourceforge.net/svnroot/ccnet/trunk@5714 8a0e9b86-a613-0410-befa-817088176213\n\nChanges:\nM\tproject/UnitTests/Core/Tasks/ReplacementDynamicValueTests.cs\n";
		const string rubyOnRailsLog = "Commit:1fbfa3e705c37656c308436f21d42b09591ba60e\r\nTime:2009-06-15 17:33:25 -0700\r\nAuthor:Yehuda Katz + Carl Lerche\r\nE-Mail:ykatz+clerche@engineyard.com\r\nMessage:More _write_layout_method removal\r\n\r\n\r\nChanges:\r\nM\tactionpack/test/controller/layout_test.rb\r\nM\tactionpack/test/controller/mime_responds_test.rb\r\n\r\nCommit:3c15cba17519e7a4acc3958662f8f3693837c179\r\nTime:2009-06-15 17:32:10 -0700\r\nAuthor:Yehuda Katz + Carl Lerche\r\nE-Mail:ykatz+clerche@engineyard.com\r\nMessage:Whoops, I guess we broke layouts ;)\r\n\r\n\r\nChanges:\r\nM\tactionpack/lib/action_controller/abstract/layouts.rb\r\nM\tactionpack/lib/action_controller/base/base.rb\r\nM\tactionpack/test/abstract_controller/abstract_controller_test.rb\r\nM\tactionpack/test/abstract_controller/layouts_test.rb\r\nM\tactionpack/test/new_base/render_text_test.rb\r\nM\tactionpack/test/new_base/test_helper.rb\r\n\r\nCommit:19c3495a671c364e0dc76c276efbcd9dc6914c74\r\nTime:2009-06-15 16:29:45 -0700\r\nAuthor:Yehuda Katz + Carl Lerche\r\nE-Mail:ykatz+clerche@engineyard.com\r\nMessage:rm -r controller/base!\r\n\r\n\r\nChanges:\r\nM\tactionmailer/lib/action_mailer/base.rb\r\nM\tactionpack/lib/action_controller.rb\r\nA\tactionpack/lib/action_controller/base/cookies.rb\r\nA\tactionpack/lib/action_controller/base/filter_parameter_logging.rb\r\nA\tactionpack/lib/action_controller/base/flash.rb\r\nA\tactionpack/lib/action_controller/base/http_authentication.rb\r\nA\tactionpack/lib/action_controller/base/mime_responds.rb\r\nA\tactionpack/lib/action_controller/base/request_forgery_protection.rb\r\nA\tactionpack/lib/action_controller/base/session_management.rb\r\nA\tactionpack/lib/action_controller/base/streaming.rb\r\nA\tactionpack/lib/action_controller/base/verification.rb\r\nA\tactionpack/lib/action_controller/legacy/layout.rb\r\nD\tactionpack/lib/action_controller/old_base/base.rb\r\nD\tactionpack/lib/action_controller/old_base/chained/benchmarking.rb\r\nD\tactionpack/lib/action_controller/old_base/chained/filters.rb\r\nD\tactionpack/lib/action_controller/old_base/chained/flash.rb\r\nD\tactionpack/lib/action_controller/old_base/cookies.rb\r\nD\tactionpack/lib/action_controller/old_base/filter_parameter_logging.rb\r\nD\tactionpack/lib/action_controller/old_base/helpers.rb\r\nD\tactionpack/lib/action_controller/old_base/http_authentication.rb\r\nD\tactionpack/lib/action_controller/old_base/layout.rb\r\nD\tactionpack/lib/action_controller/old_base/mime_responds.rb\r\nD\tactionpack/lib/action_controller/old_base/redirect.rb\r\nD\tactionpack/lib/action_controller/old_base/render.rb\r\nD\tactionpack/lib/action_controller/old_base/request_forgery_protection.rb\r\nD\tactionpack/lib/action_controller/old_base/rescue.rb\r\nD\tactionpack/lib/action_controller/old_base/responder.rb\r\nD\tactionpack/lib/action_controller/old_base/session_management.rb\r\nD\tactionpack/lib/action_controller/old_base/streaming.rb\r\nD\tactionpack/lib/action_controller/old_base/verification.rb\r\n\r\nCommit:7b1f483fda4fc8e4fc931649364a211a9f9d945f\r\nTime:2009-06-15 16:14:45 -0700\r\nAuthor:Yehuda Katz + Carl Lerche\r\nE-Mail:ykatz+clerche@engineyard.com\r\nMessage:Get all of rake tests to pass\r\n\r\n\r\nChanges:\r\nM\tactionpack/lib/action_controller.rb\r\nM\trailties/test/rails_info_controller_test.rb\r\n\r\nCommit:c7ba9aa195196d534109575affab700f7bc10984\r\nTime:2009-06-15 15:56:18 -0700\r\nAuthor:Yehuda Katz + Carl Lerche\r\nE-Mail:ykatz+clerche@engineyard.com\r\nMessage:Get the new base tests to pass\r\n\r\n\r\nChanges:\r\nM\tactionpack/test/new_base/test_helper.rb\r\n\r\nCommit:0e558f0bbdbfecf9ebc5218a78ba224a295c432d\r\nTime:2009-06-15 15:55:09 -0700\r\nAuthor:Yehuda Katz + Carl Lerche\r\nE-Mail:ykatz+clerche@engineyard.com\r\nMessage:Get the AR integration tests to pass\r\n\r\n\r\nChanges:\r\nM\tactionpack/test/abstract_unit.rb\r\n\r\nCommit:5314abed182450dbdcc25ebe601a2bbdf4cb926f\r\nTime:2009-06-15 11:59:28 -0700\r\nAuthor:Yehuda Katz + Carl Lerche\r\nE-Mail:ykatz+clerche@engineyard.com\r\nMessage:Keep ActionMailer using the old layouts code until it gets refactored.\r\n\r\n\r\nChanges:\r\nM\tactionmailer/lib/action_mailer/base.rb\r\nM\tactionpack/Rakefile\r\nM\tactionpack/lib/action_controller.rb\r\nM\tactionpack/lib/action_controller/old_base/layout.rb\r\n\r\nCommit:80d1e2778860d1825d749aad274e70e0ea810bc6\r\nTime:2009-06-15 11:50:11 -0700\r\nAuthor:Yehuda Katz + Carl Lerche\r\nE-Mail:ykatz+clerche@engineyard.com\r\nMessage:Fix the fixture path\r\n\r\n\r\nChanges:\r\nM\tactionpack/test/abstract_unit.rb\r\n\r\nCommit:a63caa4c0c2a8aabc13c354a9193ebd9c5e8ba73\r\nTime:2009-06-15 11:44:45 -0700\r\nAuthor:Yehuda Katz + Carl Lerche\r\nE-Mail:ykatz+clerche@engineyard.com\r\nMessage:Get tests to run (with failures) without old base around\r\n\r\n\r\nChanges:\r\nM\tactionmailer/lib/action_mailer/base.rb\r\nM\tactionpack/lib/action_controller.rb\r\nM\tactionpack/lib/action_controller/base/base.rb\r\nD\tactionpack/lib/action_controller/base/chained/benchmarking.rb\r\nD\tactionpack/lib/action_controller/base/chained/filters.rb\r\nD\tactionpack/lib/action_controller/base/chained/flash.rb\r\nA\tactionpack/lib/action_controller/base/compatibility.rb\r\nA\tactionpack/lib/action_controller/base/conditional_get.rb\r\nD\tactionpack/lib/action_controller/base/cookies.rb\r\nD\tactionpack/lib/action_controller/base/filter_parameter_logging.rb\r\nM\tactionpack/lib/action_controller/base/helpers.rb\r\nA\tactionpack/lib/action_controller/base/hide_actions.rb\r\nA\tactionpack/lib/action_controller/base/http.rb\r\nD\tactionpack/lib/action_controller/base/http_authentication.rb\r\nD\tactionpack/lib/action_controller/base/layout.rb\r\nA\tactionpack/lib/action_controller/base/layouts.rb\r\nD\tactionpack/lib/action_controller/base/mime_responds.rb\r\nA\tactionpack/lib/action_controller/base/rack_convenience.rb\r\nD\tactionpack/lib/action_controller/base/redirect.rb\r\nA\tactionpack/lib/action_controller/base/redirector.rb\r\nD\tactionpack/lib/action_controller/base/render.rb\r\nA\tactionpack/lib/action_controller/base/render_options.rb\r\nA\tactionpack/lib/action_controller/base/renderer.rb\r\nD\tactionpack/lib/action_controller/base/request_forgery_protection.rb\r\nA\tactionpack/lib/action_controller/base/rescuable.rb\r\nD\tactionpack/lib/action_controller/base/rescue.rb\r\nD\tactionpack/lib/action_controller/base/responder.rb\r\nA\tactionpack/lib/action_controller/base/session.rb\r\nD\tactionpack/lib/action_controller/base/session_management.rb\r\nD\tactionpack/lib/action_controller/base/streaming.rb\r\nA\tactionpack/lib/action_controller/base/testing.rb\r\nA\tactionpack/lib/action_controller/base/url_for.rb\r\nD\tactionpack/lib/action_controller/base/verification.rb\r\nD\tactionpack/lib/action_controller/new_base.rb\r\nD\tactionpack/lib/action_controller/new_base/base.rb\r\nD\tactionpack/lib/action_controller/new_base/compatibility.rb\r\nD\tactionpack/lib/action_controller/new_base/conditional_get.rb\r\nD\tactionpack/lib/action_controller/new_base/helpers.rb\r\nD\tactionpack/lib/action_controller/new_base/hide_actions.rb\r\nD\tactionpack/lib/action_controller/new_base/http.rb\r\nD\tactionpack/lib/action_controller/new_base/layouts.rb\r\nD\tactionpack/lib/action_controller/new_base/rack_convenience.rb\r\nD\tactionpack/lib/action_controller/new_base/redirector.rb\r\nD\tactionpack/lib/action_controller/new_base/render_options.rb\r\nD\tactionpack/lib/action_controller/new_base/renderer.rb\r\nD\tactionpack/lib/action_controller/new_base/rescuable.rb\r\nD\tactionpack/lib/action_controller/new_base/session.rb\r\nD\tactionpack/lib/action_controller/new_base/testing.rb\r\nD\tactionpack/lib/action_controller/new_base/url_for.rb\r\nA\tactionpack/lib/action_controller/old_base.rb\r\nA\tactionpack/lib/action_controller/old_base/base.rb\r\nA\tactionpack/lib/action_controller/old_base/chained/benchmarking.rb\r\nA\tactionpack/lib/action_controller/old_base/chained/filters.rb\r\nA\tactionpack/lib/action_controller/old_base/chained/flash.rb\r\nA\tactionpack/lib/action_controller/old_base/cookies.rb\r\nA\tactionpack/lib/action_controller/old_base/filter_parameter_logging.rb\r\nA\tactionpack/lib/action_controller/old_base/helpers.rb\r\nA\tactionpack/lib/action_controller/old_base/http_authentication.rb\r\nA\tactionpack/lib/action_controller/old_base/layout.rb\r\nA\tactionpack/lib/action_controller/old_base/mime_responds.rb\r\nA\tactionpack/lib/action_controller/old_base/redirect.rb\r\nA\tactionpack/lib/action_controller/old_base/render.rb\r\nA\tactionpack/lib/action_controller/old_base/request_forgery_protection.rb\r\nA\tactionpack/lib/action_controller/old_base/rescue.rb\r\nA\tactionpack/lib/action_controller/old_base/responder.rb\r\nA\tactionpack/lib/action_controller/old_base/session_management.rb\r\nA\tactionpack/lib/action_controller/old_base/streaming.rb\r\nA\tactionpack/lib/action_controller/old_base/verification.rb\r\nM\tactionpack/test/abstract_unit.rb\r\nD\tactionpack/test/new_base/abstract_unit.rb\r\nA\tactionpack/test/old_base/abstract_unit.rb\r\n\r\nCommit:614374b3e5b19b737175a82c6dad2f146800eef1\r\nTime:2009-06-15 11:31:52 -0700\r\nAuthor:Yehuda Katz + Carl Lerche\r\nE-Mail:ykatz+clerche@engineyard.com\r\nMessage:Require missing file in AS\r\n\r\n\r\nChanges:\r\nM\tactivesupport/lib/active_support/core_ext/proc.rb\r\n\r\nCommit:64ae5b56fff7c51fd436aaf81b8f30488cce3a2d\r\nTime:2009-06-15 11:26:47 -0700\r\nAuthor:Yehuda Katz + Carl Lerche\r\nE-Mail:ykatz+clerche@engineyard.com\r\nMessage:Fix failing tests in new callbacks\r\n\r\n\r\nChanges:\r\nM\tactivesupport/lib/active_support/new_callbacks.rb\r\n\r\nCommit:5a8a550a45c5ca7abc9785ed180d5f46189c9958\r\nTime:2009-06-15 11:21:08 -0700\r\nAuthor:Yehuda Katz + Carl Lerche\r\nE-Mail:ykatz+clerche@engineyard.com\r\nMessage:Finish making things pass with updated internal content_type semantics\r\n\r\n\r\nChanges:\r\nM\tactionpack/lib/action_controller/base/mime_responds.rb\r\nM\tactionpack/lib/action_controller/base/streaming.rb\r\nM\tactionpack/lib/action_controller/new_base/rack_convenience.rb\r\nM\tactionpack/lib/action_controller/testing/process.rb\r\nM\tactionpack/lib/action_dispatch/http/response.rb\r\nM\tactionpack/test/controller/content_type_test.rb\r\nM\tactionpack/test/controller/send_file_test.rb\r\n\r\nCommit:c50b03b754948b676b74c334edfb277fa45c1d14\r\nTime:2009-06-15 10:23:23 -0500\r\nAuthor:Joshua Peek\r\nE-Mail:josh@joshpeek.com\r\nMessage:Add :concat option to asset tag helpers to force concatenation.\r\n\r\nThis is useful for working around IE's stylesheet limit.\r\n\r\n  stylesheet_link_tag :all, :concat => true\r\nChanges:\r\nM\tactionpack/lib/action_view/helpers/asset_tag_helper.rb\r\nM\tactionpack/test/template/asset_tag_helper_test.rb\r\n";
		const string unicodeFileNameLog = "Commit:e3d76a47ceb90410323773c80da19334fab549dc\r\nTime:2012-08-28 15:58:17 +0100\r\nAuthor:Robert Adams\r\nE-Mail:robhughadams@hotmail.com\r\nMessage:Update french pdf assets\r\n\r\n\r\nChanges:\r\nA\t\"/site/assets/pdfs/BWMF Politique de gestion des conflits d'int\\303\\203\\302\\251r\\303\\203\\302\\252ts.pdf\"\r\nD\t\"/site/assets/pdfs/BWMF Politique de gestion des conflits d'int\\303\\251r\\303\\252ts.pdf\"\r\nA\t\"/site/assets/pdfs/BWMF Politique de meilleure ex\\303\\203\\302\\251cution.pdf\"\r\nD\t\"/site/assets/pdfs/BWMF Politique de meilleure ex\\303\\251cution.pdf\"\r\n\r\n";
		private readonly GitHistoryParser git = new GitHistoryParser();

		[Fact]
		public void ParsingEmptyLogProducesNoModifications()
		{
			Modification[] modifications = git.Parse(new StringReader(string.Empty), DateTime.Now, DateTime.Now);
			Assert.True(0 == modifications.Length, "#A1");
            Assert.True(true);
        }

		[Fact]
		public void ParsingSingleLogMessageProducesOneModification()
		{
			Modification[] modifications = git.Parse(new StringReader(oneEntryLog), new DateTime(2009, 06, 13, 10, 00, 00, DateTimeKind.Utc).ToLocalTime(), DateTime.Now);
			Assert.True(1 == modifications.Length, "#B1");

			Modification mod = modifications[0];
			Assert.True("24024978a9823df37a23afe533ad1c81f62467ed" == mod.ChangeNumber, "#B2");
			Assert.True(mod.Comment.StartsWith("Expanded the tests"), "#B2");
			Assert.True(mod.Comment.Contains("git-svn-id"), "#B3");
			Assert.True("cj_sutherland@8a0e9b86-a613-0410-befa-817088176213" == mod.EmailAddress, "#B4");
			Assert.True("ReplacementDynamicValueTests.cs" == mod.FileName, "#B5");
			Assert.True("project/UnitTests/Core/Tasks" == mod.FolderName, "#B6");
			Assert.Null(mod.IssueUrl);
			Assert.True(new DateTime(2009, 6, 13, 10, 37, 42, DateTimeKind.Utc) == mod.ModifiedTime.ToUniversalTime(), "#B8");
			Assert.True("Modified" == mod.Type, "#B9");
			Assert.Null(mod.Url);
			Assert.True("cj_sutherland" == mod.UserName, "#B10");
			Assert.Empty(mod.Version);
		}

        [Fact(Skip = "")]
        //[Ignore("TODO: provide a reason")]
		public void ParsingLogWithLF()
		{
			Modification[] modifications = git.Parse(File.OpenText("CCNet.git.log.txt"), new DateTime(2009, 01, 01, 10, 00, 00, DateTimeKind.Utc), new DateTime(2009, 01, 31, 10, 00, 00, DateTimeKind.Utc));
			Assert.True(85 == modifications.Length, "#C1");
			Assert.True("dec91e2a00ce14c091330f41ed5055627272022e" == Modification.GetLastChangeNumber(modifications), "C2");

			Modification mod = modifications[0];
			Assert.True("dec91e2a00ce14c091330f41ed5055627272022e" == mod.ChangeNumber, "#C3");
			Assert.True(mod.Comment.StartsWith("CCNet-748 Provide better trigger exception logging"), "#C4");
			Assert.True("willemsruben@8a0e9b86-a613-0410-befa-817088176213" == mod.EmailAddress, "#C5");
			Assert.True("Project.cs" == mod.FileName, "#C6");
			Assert.True("project/core" == mod.FolderName, "#C7");
			Assert.Null(mod.IssueUrl);
			Assert.True(new DateTime(2009, 1, 30, 13, 39, 57, DateTimeKind.Utc) == mod.ModifiedTime.ToUniversalTime(), "#C9");
			Assert.True("Modified" == mod.Type, "#C10");
			Assert.Null(mod.Url);
			Assert.True("willemsruben" == mod.UserName, "#C12");
			Assert.Empty(mod.Version);

			mod = modifications[55];
			Assert.True("f8f963749bb64d3867599eb660fafbfc18a8ea0a" == mod.ChangeNumber, "#C14");
			Assert.True("Added the validator to the installer.\n\ngit-svn-id: https://ccnet.svn.sourceforge.net/svnroot/ccnet/trunk@4533 8a0e9b86-a613-0410-befa-817088176213" == mod.Comment, "#C15");
			Assert.True("cj_sutherland@8a0e9b86-a613-0410-befa-817088176213" == mod.EmailAddress, "#C16");
			Assert.True("ccnet.sln" == mod.FileName, "#C17");
			Assert.True("project" == mod.FolderName, "#C18");
			Assert.Null(mod.IssueUrl);
			Assert.True(new DateTime(2009, 1, 8, 20, 52, 37, DateTimeKind.Utc) == mod.ModifiedTime.ToUniversalTime(), "#C20");
			Assert.True("Modified" == mod.Type, "#C21");
			Assert.Null(mod.Url);
			Assert.True("cj_sutherland" == mod.UserName, "#C23");
			Assert.Empty(mod.Version);
		}

        [Fact(Skip = "")]
        //[Ignore("TODO: provide a reason")]
		public void ParsingLargeGitLog()
		{
			Modification[] modifications = git.Parse(File.OpenText("CCNet.git.log.txt"), DateTime.MinValue, DateTime.MaxValue);
			Assert.True(22212 == modifications.Length, "#D1");
			Assert.True("dcbc5553fbc7e18905c47ae0eb10d15072e55cae" == Modification.GetLastChangeNumber(modifications), "D2");

			Modification mod = modifications[0];
			Assert.True("dcbc5553fbc7e18905c47ae0eb10d15072e55cae" == mod.ChangeNumber, "#D3");
			Assert.True(mod.Comment.StartsWith("Updated PowerShellTaskTest and NDepend"), "#D4");
			Assert.True("cj_sutherland@8a0e9b86-a613-0410-befa-817088176213" == mod.EmailAddress, "#D5");
			Assert.True("NDependTaskTests.cs" == mod.FileName, "#D6");
			Assert.True("project/UnitTests/Core/Tasks" == mod.FolderName, "#D7");
			Assert.Null(mod.IssueUrl);
			Assert.True(new DateTime(2009, 6, 14, 11, 02, 31, DateTimeKind.Utc) == mod.ModifiedTime.ToUniversalTime(), "#D9");
			Assert.True("Modified" == mod.Type, "#D10");
			Assert.Null(mod.Url);
			Assert.True("cj_sutherland" == mod.UserName, "#D12");
			Assert.Empty(mod.Version);

			mod = modifications[22211];
			Assert.True("0653fe2541cde05dc6098fe8afac79d1b4e0a62f", mod.ChangeNumber, "#D14");
			Assert.True("first checkin\n\ngit-svn-id: https://ccnet.svn.sourceforge.net/svnroot/ccnet/trunk@2 8a0e9b86-a613-0410-befa-817088176213", mod.Comment, "#D15");
			Assert.True("mikeroberts@8a0e9b86-a613-0410-befa-817088176213", mod.EmailAddress, "#D16");
			Assert.True("updateConfig.bat", mod.FileName, "#D17");
			Assert.Empty(mod.FolderName, "#D18");
			Assert.Null(mod.IssueUrl);
			Assert.True(new DateTime(2003, 4, 22, 16, 49, 49, DateTimeKind.Utc), mod.ModifiedTime.ToUniversalTime(), "#D20");
			Assert.True("Added", mod.Type, "#D21");
			Assert.Null(mod.Url);
			Assert.True("mikeroberts", mod.UserName, "#D23");
			Assert.Empty(mod.Version, "#D24");
		}

		[Fact]
		public void ParsingLogWithCRLF()
		{
			Modification[] modifications = git.Parse(new StringReader(rubyOnRailsLog), new DateTime(2009, 06, 13, 10, 00, 00, DateTimeKind.Utc), DateTime.Now);
			Assert.True(129, modifications.Length, "#E1");

			Modification mod = modifications[0];
			Assert.True("1fbfa3e705c37656c308436f21d42b09591ba60e" == mod.ChangeNumber, "#E2");
			Assert.True("More _write_layout_method removal" == mod.Comment, "#E3");
			Assert.True("ykatz+clerche@engineyard.com" == mod.EmailAddress, "#E4");
			Assert.True("layout_test.rb" == mod.FileName, "#E5");
			Assert.True("actionpack/test/controller" == mod.FolderName, "#E6");
			Assert.Null(mod.IssueUrl);
			Assert.True(new DateTime(2009, 6, 16, 0, 33, 25, DateTimeKind.Utc) == mod.ModifiedTime.ToUniversalTime(), "#E8");
			Assert.True("Modified" == mod.Type, "#E9");
			Assert.Null(mod.Url);
			Assert.True("Yehuda Katz + Carl Lerche" == mod.UserName, "#E11");
			Assert.Empty(mod.Version);

			mod = modifications[34];
			Assert.True("19c3495a671c364e0dc76c276efbcd9dc6914c74" == mod.ChangeNumber, "#E13");
			Assert.True("rm -r controller/base!" == mod.Comment, "#E14");
			Assert.True("ykatz+clerche@engineyard.com" == mod.EmailAddress, "#E15");
			Assert.True("responder.rb" == mod.FileName, "#E16");
			Assert.True("actionpack/lib/action_controller/old_base" == mod.FolderName, "#E17");
			Assert.Null(mod.IssueUrl);
			Assert.True(new DateTime(2009, 6, 15, 23, 29, 45, DateTimeKind.Utc) == mod.ModifiedTime.ToUniversalTime(), "#E19");
			Assert.True("Deleted" == mod.Type, "#E20");
			Assert.Null(mod.Url);
			Assert.True("Yehuda Katz + Carl Lerche" == mod.UserName, "#E22");
			Assert.Empty(mod.Version);
		}

		[Fact]
		public void ParsingLogWithUnicodeFilePath()
		{
			var modifications = git.Parse(new StringReader(unicodeFileNameLog), new DateTime(2009, 06, 13, 10, 00, 00, DateTimeKind.Utc).ToLocalTime(), DateTime.Now);
			Assert.Equal(4,modifications.Length);

			var mod = modifications[0];
			var combinedPath = Path.Combine(mod.FolderName, mod.FileName);
			Assert.Matches(combinedPath, Path.Combine("/site/assets/pdfs", "BWMF Politique de gestion des conflits d'int\\303\\203\\302\\251r\\303\\203\\302\\252ts.pdf"));
		}
	}
}
