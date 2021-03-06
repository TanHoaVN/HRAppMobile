﻿using HRApp.Constant;
using HRApp.Models;
using HRApp.Services;
using HRApp.Views;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HRApp.ViewModels
{
	public class HomeViewModel : ViewModelBase
	{
        ODataService oDataService;
        public HomeViewModel(INavigationService navigationService)
           : base(navigationService)
        {
            Title = "Trang Thông Tin Cá Nhân";
            ViewInformationDetail = new DelegateCommand(async () => await ViewInformationDetailExecute());
            UploadImageCommand = new DelegateCommand(async () => await UploadImageCommandExecute());
        }
        private async Task ViewInformationDetailExecute()
        {
            await NavigationService.NavigateAsync("InformationDetail");
        }
        string _FullName;
        public string FullName
        {
            get => _FullName;
            set => SetProperty(ref _FullName, value);
        }
        private string _EmployeeCode;
        public string EmployeeCode
        {
            get => _EmployeeCode;
            set
            {
                SetProperty(ref _EmployeeCode, value);
                RaisePropertyChanged("EmployeeCode");
            }
        }
        private string _Department;
        public string Department
        {
            get => _Department;
            set => SetProperty (ref _Department, value);
        }
        private string _Supervisor;
        public string Supervisor
        {
            get => _Supervisor;
            set => SetProperty (ref _Supervisor, value);
        }
        private ImageSource _avatar;
        public ImageSource avatar
        {
            get => _avatar;
            set => SetProperty(ref _avatar, value);
        }
        private NhanVien _nhanVien;
        public NhanVien nhanVien
        {
            get => _nhanVien;
            set
            {
                SetProperty(ref _nhanVien, value);
                RaisePropertyChanged("nhanVien");
            }
        }
        public DelegateCommand ViewInformationDetail { get; set; }
        public override async void OnNavigatingTo(INavigationParameters parameters)
        {
            this.nhanVien = parameters.GetValue<NhanVien>("nhanVien");
            this.oDataService = parameters.GetValue<ODataService>("service");
            this.EmployeeCode = nhanVien.userName;
            try
            { 
                this.nhanVien = await oDataService.GetEmployee(this.EmployeeCode);
                this.FullName = this.nhanVien.Name;
                this.Department = this.nhanVien.department;
                this.Supervisor = this.nhanVien.supervisor;
                this.avatar = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(this.nhanVien.image)));
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        public DelegateCommand UploadImageCommand { get; set; }
        public async Task UploadImageCommandExecute()
        {
            await CrossMedia.Current.Initialize();
            if(!CrossMedia.Current.IsPickPhotoSupported)
            {
                await App.Current.MainPage.DisplayAlert("Cập Nhật Hình ảnh", "Thiết Bị Không Hỗ Trợ Truy Cập Hình Ảnh", "Xác Nhận");
            }
            else
            {
                var mediaOptions = new PickMediaOptions
                {
                    PhotoSize = PhotoSize.Medium
                };
                var selectedImageFile = await CrossMedia.Current.PickPhotoAsync(mediaOptions);
                if(selectedImageFile != null)
                {
                    byte[] image = new byte[selectedImageFile.GetStream().Length];
                    selectedImageFile.GetStream().Read(image, 0, image.Length);
                    await oDataService.UploadImage(this.nhanVien.Id, image);
                    await App.Current.MainPage.DisplayAlert("Cập Nhật Hình ảnh", "Cập Nhật Hình Ảnh Thành Công", "Xác Nhận");
                    try
                    {
                        this.nhanVien = await oDataService.GetEmployee(this.EmployeeCode);
                        this.avatar = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(this.nhanVien.image)));
                    }
                    catch (Exception exception)
                    {
                        throw exception;
                    }
                }
            }
        }
        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            NavigationParameters parameter = new NavigationParameters
            {
                {"nhanVien",this.nhanVien},
                {"service",this.oDataService}
            };
        }
    }
}
