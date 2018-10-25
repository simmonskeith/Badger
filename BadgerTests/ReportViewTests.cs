
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Badger.Runner.Views;
using System.Windows.Forms;
using Badger.Runner.Interfaces;
using Badger.Runner.Presenters;
using Xunit;
using NSubstitute;
using FluentAssertions;

namespace Badger.Tests
{
    [Trait("Category", "Report Viewer")]
    public class ReportViewPresenterTests
    {
        private IReportView _mockView;
        private IMessageBoxService _mockOKDialog;
        private ReportViewPresenter _presenter;

        public ReportViewPresenterTests()
        {
            _mockView = Substitute.For<IReportView>();
            _mockView.When(m=>m.ShowDialog()).Do(x => 
                _mockView.OnFormLoad += Raise.EventWith(new object(), new EventArgs()));

            _mockOKDialog = Substitute.For<IMessageBoxService>();
            _presenter = new ReportViewPresenter(_mockView, _mockOKDialog);
            _presenter.Url = "test";
        }
        
        [Theory]
        [InlineData("test")]
        [InlineData(null)]
        [InlineData("")]
        public void ReportView_OnLoad_ControlsShowDefaultStates(string url)
        {
            int okDialogCalls = string.IsNullOrEmpty(url) ? 1 : 0;
            _presenter.Url = url;
            _presenter.ShowView();
            _mockView.BackButtonEnabled.Should().BeFalse();
            _mockView.ForwardButtonEnabled.Should().BeFalse();
            _mockOKDialog.Received(okDialogCalls).Show(Arg.Any<string>(), Arg.Any<string>(), MessageBoxButtons.OK);
        }

        
        [Fact]
        public void ReportView_BackButtonEnabled_WhenAbleToGoBack()
        {
            _presenter.ShowView();
            _mockView.CanGoBack.Returns(true);

            _mockView.CanGoBackChanged += Raise.EventWith(null, EventArgs.Empty);

            _mockView.BackButtonEnabled.Should().BeTrue();
            _mockView.ForwardButtonEnabled.Should().BeFalse();
        }
        
        [Fact]
        public void ReportView_ForwardButtonEnabled_WhenAbleToGoForward()
        {
            _presenter.ShowView();
            _mockView.CanGoForward.Returns(true);

            _mockView.CanGoForwardChanged += Raise.EventWith(null, EventArgs.Empty);

            _mockView.BackButtonEnabled.Should().BeFalse();
            _mockView.ForwardButtonEnabled.Should().BeTrue();
        }
        
    }
}
